using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SamCorp.WPF.Extensions;

namespace CampaignMaster.Controls {

    /// <summary>
    /// Interaction logic for NameGenerator.xaml
    /// </summary>
    public partial class NameGenerator : UserControl {

        private const string FirstNameKey = "Firstname";
        private const string LastNameKey = "Lastname";

        private Dictionary<string, Dictionary<string, List<string>>> NameLists = new();
        private List<string> KeysForAllRandom = new() { "male", "female", "elf", "gnome", "halfling" };

        public NameGenerator() {
            InitializeComponent();
        }

        private async Task<Dictionary<string, List<string>>> GetNameList(string type) {
            if (!NameLists.ContainsKey(type)) {
                NameLists[type] = new Dictionary<string, List<string>> {
                    [FirstNameKey] = new(),
                    [LastNameKey] = new()
                };

                var csvContent = await Assembly.GetExecutingAssembly().ReadResourceFileAsync(type, "csv");
                if (csvContent.IsNullOrEmpty()) {
                    return NameLists[type];
                }

                var lines = csvContent.Split(Environment.NewLine);
                foreach (var line in lines) {
                    var lineSplit = line.Split(';');
                    if(lineSplit.Length != 2)
                    {
                        continue;
                    }

                    var firstName = lineSplit[0];
                    var lastName = lineSplit[1];

                    NameLists[type][FirstNameKey].Add(firstName);
                    NameLists[type][LastNameKey].Add(lastName);
                }
            }

            return NameLists[type];
        }

        private async void GenerateNameButtonClick(object sender, RoutedEventArgs e) {
            if (sender is not Button btn) {
                return;
            }

            var nameList = await GetNameList(btn.Tag.ToString());
            if (nameList[FirstNameKey].Count == 0) {
                return;
            }

            var rnd = new Random();
            var firstNameIndex = rnd.Next(0, nameList[FirstNameKey].Count - 1);
            var lastNameIndex = rnd.Next(0, nameList[LastNameKey].Count - 1);

            txtName.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nameList[FirstNameKey][firstNameIndex] + " " + nameList[LastNameKey][lastNameIndex]);
        }

        private async void FullRandomButtonClick(object sender, RoutedEventArgs e) {
            if (sender is not Button btn) {
                return;
            }

            var rnd = new Random();

            var firstNameListIndex = rnd.Next(0, KeysForAllRandom.Count - 1);
            var firstNameList = await GetNameList(KeysForAllRandom[firstNameListIndex]);
            if (firstNameList[FirstNameKey].Count == 0) {
                return;
            }

            var lastNameListIndex = rnd.Next(0, KeysForAllRandom.Count - 1);
            var lastNameList = await GetNameList(KeysForAllRandom[firstNameListIndex]);
            if (lastNameList[LastNameKey].Count == 0) {
                return;
            }

            var firstNameIndex = rnd.Next(0, firstNameList[FirstNameKey].Count - 1);
            var lastNameIndex = rnd.Next(0, lastNameList[LastNameKey].Count - 1);

            txtName.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(firstNameList[FirstNameKey][firstNameIndex] + " " + lastNameList[LastNameKey][lastNameIndex]);
        }

    }

}