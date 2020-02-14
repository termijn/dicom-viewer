using System;

namespace Entities
{
    public class Patient : Bindable
    {
        private string _givenName;
        private string _familyName;
        private string _middleName;
        private string _id;
        private DateTime? _dateOfBirth;

        public string GivenName
        {
            get => _givenName;
            set => SetProperty(ref _givenName, value);
        }

        public string FamilyName
        {
            get => _familyName;
            set => SetProperty(ref _familyName, value);
        }

        public string MiddleName
        {
            get => _middleName;
            set => SetProperty(ref _middleName, value);
        }

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set => SetProperty(ref _dateOfBirth, value);
        }

        public string FullReadableName
        {
            get
            {
                string result = "";
                if (!string.IsNullOrEmpty(FamilyName))
                {
                    result += FamilyName;
                }
                if (!string.IsNullOrEmpty(GivenName))
                {
                    result += ", " + GivenName;
                }
                if (!string.IsNullOrEmpty(MiddleName))
                {
                    result += ", " + MiddleName;
                }
                return result;
            }
        }

        public void ParseDicomDateOfBirth(string date)
        {
            if (date.Length != 8) return;

            int year = 0;
            int month = 0;
            int day = 0;

            int.TryParse(date.Substring(0, 4), out year);
            int.TryParse(date.Substring(4, 2), out month);
            int.TryParse(date.Substring(6, 2), out day);
            DateOfBirth = new DateTime(year, month, day);
        }

        public void ParseDicomPersonName(string name)
        {
            var tokens = name.Split('^');

            FamilyName = tokens.Length > 0 ? tokens[0] : string.Empty;
            GivenName = tokens.Length > 1 ? tokens[1] : string.Empty;
            MiddleName = tokens.Length > 2 ? tokens[2] : string.Empty;
        }
    }
}
