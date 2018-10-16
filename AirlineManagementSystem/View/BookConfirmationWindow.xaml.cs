﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AirportManagerSystem.Model;

namespace AirportManagerSystem.View
{
    /// <summary>
    /// Interaction logic for BookConfirmationWindow.xaml
    /// </summary>
    public partial class BookConfirmationWindow : Window
    {
        private List<Country> countries;
        List<Ticket> passengers = new List<Ticket>();

        public CabinType Cabin { get; internal set; }
        public int Numpass { get; internal set; }
        internal Flight ReturnFlight { get; set; }
        internal Flight OutboundFlight { get; set; }
        private int SelecetedIndexPassenger = -1;
        public BookConfirmationWindow()
        {
            InitializeComponent();
            this.Loaded += BookConfirmationWindow_Loaded;
            dgPassengers.SelectionChanged += DgPassengers_SelectionChanged;
        }

        private void DgPassengers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SelecetedIndexPassenger = dgPassengers.Items.IndexOf(dgPassengers.CurrentItem);
            }
            catch (Exception)
            {
            }
        }

        private void BookConfirmationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            countries = Db.Context.Countries.ToList();
            cbPassportCountry.ItemsSource = countries;
            cbPassportCountry.DisplayMemberPath = "Name";
            cbPassportCountry.SelectedIndex = 0;

            LoadFlightDetail(stpOutboundFlight);
            if (ReturnFlight != null)
            {
                LoadFlightDetail(stpReturnFlight);
                grReturnFlight.Visibility = Visibility.Visible;
            }
        }

        private void LoadFlightDetail(StackPanel stpFlight)
        {
            try
            {
                foreach (var item in OutboundFlight.Flights)
                {
                    stpFlight.Children.Add(new UcFlightDetail(item, Cabin));
                }
            }
            catch (Exception)
            {
            }
        }

        private void btnAddPassenger_Click(object sender, RoutedEventArgs e)
        {
            SelecetedIndexPassenger = -1;
            if (txtFirstName.Text == "" || txtLastName.Text == "" || txtPassportNumber.Text == "" || txtPhone.Text == "")
            {
                MessageBox.Show("All field are required", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                if (dpBirthdate.SelectedDate.Value >= DateTime.Now.Date)
                {
                    MessageBox.Show("Birthdate is invalid", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Please Select birthdate!", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            if (passengers.Select(t => t.PassportNumber).Contains(txtPassportNumber.Text))
            {
                MessageBox.Show("This passport number was used in this flight", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (var item in OutboundFlight.Flights)
            {
                if (item.Tickets.Where(t => t.Confirmed).Select(t => t.PassportNumber).Contains(txtPassportNumber.Text))
                {
                    MessageBox.Show("This passport number was used in this flight", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            try
            {
                foreach (var item in ReturnFlight.Flights)
                {
                    if (item.Tickets.Where(t => t.Confirmed).Select(t => t.PassportNumber).Contains(txtPassportNumber.Text))
                    {
                        MessageBox.Show("This passport number was used in this flight", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            catch (Exception)
            {
            }

            if (Regex.IsMatch(txtPhone.Text, @"\D"))
            {
                MessageBox.Show("Phone is invalid", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (dgPassengers.Items.Count == Numpass)
            {
                MessageBox.Show($"Only {Numpass} passenger(s) can book ticket", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            passengers.Add(new Ticket()
            {
                Firstname = txtFirstName.Text,
                Lastname = txtLastName.Text,
                PassportNumber = txtPassportNumber.Text,
                Country = countries[cbPassportCountry.SelectedIndex],
                PassportCountryID = countries[cbPassportCountry.SelectedIndex].ID,
                Phone = txtPhone.Text,
                Birthdate = dpBirthdate.SelectedDate.Value
            });

            dgPassengers.ItemsSource = null;
            dgPassengers.ItemsSource = passengers;
        }

        private void btnBackToSearch_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnRemovePassenger_Click(object sender, RoutedEventArgs e)
        {
            if (SelecetedIndexPassenger != -1)
            {
                passengers.RemoveAt(SelecetedIndexPassenger);
                dgPassengers.ItemsSource = null;
                dgPassengers.ItemsSource = passengers;
                SelecetedIndexPassenger = -1;
            }
            else
            {
                MessageBox.Show("Choose a passenger!", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBookConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (dgPassengers.Items.Count< Numpass)
            {
                MessageBox.Show($"Not enough {Numpass} passenger(s)", "Message", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<Ticket> tickets = new List<Ticket>();
            foreach (var p in passengers)
            {
                foreach (var item in OutboundFlight.Flights)
                {
                    Ticket t = new Ticket()
                    {
                        Firstname = p.Firstname,
                        Lastname = p.Lastname,
                        PassportNumber = p.PassportNumber,
                        PassportCountryID = p.PassportCountryID,
                        Phone = p.Phone,
                        Schedule = item,
                        CabinType = Cabin,
                        Confirmed = true,
                        UserID = 1,
                        Birthdate = p.Birthdate
                    };

                    tickets.Add(t);
                }
                try
                {
                    foreach (var item in ReturnFlight.Flights)
                    {
                        Ticket t = new Ticket()
                        {
                            Firstname = p.Firstname,
                            Lastname = p.Lastname,
                            PassportNumber = p.PassportNumber,
                            PassportCountryID = p.PassportCountryID,
                            Phone = p.Phone,
                            Schedule = item,
                            CabinType = Cabin,
                            Confirmed = true,
                            UserID = 1,
                            Birthdate = p.Birthdate
                        };

                        tickets.Add(t);
                    }
                }
                catch (Exception)
                {
                }
            }

            BillingConfirmationWindow wBillingCofirm = new BillingConfirmationWindow();
            wBillingCofirm.Tickets = tickets;
            wBillingCofirm.TotalPrice = OutboundFlight.Price * Numpass;
            if (ReturnFlight != null) wBillingCofirm.TotalPrice += ReturnFlight.Price * Numpass;

            this.Hide();
            wBillingCofirm.ShowDialog();

            if (wBillingCofirm.IsConfirm)
                this.Close();
            else this.Show();
        }
    }
}
