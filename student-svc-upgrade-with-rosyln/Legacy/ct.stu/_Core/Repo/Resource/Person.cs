using ct._Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;

namespace ct._Core.Repo.Resource
{
    public enum GenderEnum { Female = 0, Male = 1, NoInfo = 90 }

    public interface IPerson
    {
        string Name { get; set; }
        string Title { get; set; }
        GenderEnum? Gender { get; set; }
        DateTime? DOB { get; set; }
        string Email { get; set; }
        string Mobile { get; set; }
    }

    public class Person : Entity, IPerson
    {
        public Person()
        {
            Contacts = new List<Contact>();
            Addresses = new List<Address>();
            Attributes = new Dictionary<string, object>();
            Tags = new List<string>();
            Occupations = new List<Occupation>();
        }

        [Required]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        /// <summary>
        ///     Sets FirstName, MiddleName and LastName by splitting the value on whitespace.
        /// </summary>
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(FirstName)) FirstName = string.Empty;
                if (string.IsNullOrEmpty(MiddleName)) MiddleName = string.Empty;
                if (string.IsNullOrEmpty(LastName)) LastName = string.Empty;
                var s = string.Join(" ", FirstName.Trim(), (MiddleName ?? "").Trim(), LastName.Trim());
                return Regex.Replace(s, @"\s+", " "); //replace multiple whitespaces with single space
            }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                value = value.Trim();
                //replace special characters (any non alphatbets) with space
                value = Regex.Replace(value, @"[^a-zA-Z']+", " ");
                //replace multiple whitespaces with a single space
                //ref:http://stackoverflow.com/questions/206717/how-do-i-replace-multiple-spaces-with-a-single-space-in-c
                var options = RegexOptions.None;
                var regex = new Regex(@"[ ]{2,}", options);
                var s = regex.Replace(value, @" ");

                var arr = s.Split(null); //split on whitespace
                FirstName = arr[0];
                LastName = arr[arr.Length - 1];
                MiddleName = string.Empty;
                if (arr.Length > 2)
                {
                    var midArray = arr.Skip(1).Take(arr.Length - 2);
                    MiddleName = string.Join(" ", midArray);
                }
                //Swap first & last if first name happens to be an initial
                // if (FirstName.Length == 1 && LastName.Length > 1)
                // {
                //     var f = FirstName;
                //     FirstName = LastName;
                //     LastName = f;
                // }

                var cultureInfo   = Thread.CurrentThread.CurrentCulture;  
                var textInfo = cultureInfo.TextInfo; 
                FirstName = textInfo.ToTitleCase(FirstName);
                MiddleName = textInfo.ToTitleCase(MiddleName);
                LastName = textInfo.ToTitleCase(LastName);
            }
        }
        /// <summary>
        /// Title Prefix like Dr CA etc
        /// </summary>
        public string Title { get; set; }
        public GenderEnum? Gender { get; set; }
        public DateTime? DOB { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        private string _virtualId;
        public string VirtualId
        {
            get
            {
                if (string.IsNullOrEmpty(_virtualId))
                {
                    if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(Name.Trim())) return "";
                    else
                    {
                        var username = string.Join(".", FirstName.Trim(), LastName.Trim(), Id.Substring(0,4));
                        var virtualId = string.Join("@", username.ToLower().Trim(), "campustrack.net");
                        return virtualId;
                    }
                }
                else return _virtualId;
            }
            set
            {
                if (string.IsNullOrEmpty(value) || !value.Contains("@campustrack.net")) return;
                _virtualId = value.Trim();
            }
        }

        /// <summary>
        /// To be used for resetting names of linked person(s) such as Father / Mother / Guardian. Not to be used elsewhere.
        /// </summary>
        public void ResetName(string name = "")
        {
            if (string.IsNullOrEmpty(name))
            {
                this.FirstName = string.Empty;
                this.MiddleName = string.Empty;
                this.LastName = string.Empty;
            }
            else this.Name = name;
        }

        public List<Contact> Contacts { get; set; }
        public List<Address> Addresses { get; set; }
        public List<string> Tags { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public List<Occupation> Occupations { get; set; }
        /// <summary>
        /// PAN no
        /// </summary>
        /// <value></value>
        public string PAN { get; set; }

        [JsonIgnore]
        public bool HasValidMobile
        {
            get
            {
                try
                {
                    return
                        Contacts.Where(
                            o =>
                                o.ContactType == ContactTypeEnum.Mobile && o.Primary.Equals(true) && o.Info.Length == 10)
                            .FirstOrDefault() != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        [JsonIgnore]
        public bool HasValidEmail
        {
            get
            {
                try
                {
                    var contact =
                        Contacts.Where(o => o.ContactType == ContactTypeEnum.Email && o.Primary.Equals(true))
                            .FirstOrDefault();
                    if (contact == null) return false;
                    var addr = new MailAddress(contact.Info); //will throw if email is invalid
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public void SanitizeName()
        {
            if (string.IsNullOrEmpty(Name)) return;
            FirstName = Helper.Capitalize(FirstName);
            MiddleName = Helper.Capitalize(MiddleName);
            LastName = Helper.Capitalize(LastName);
        }

        public void SetHomeAddress(string addr)
        {
            var e = string.Empty;
            SetHomeAddress(addr, e, e, e, e, e);
        }

        public void SetHomeAddress(string line1, string line2, string city, string state, string country, string pincode)
        {
            var homeAddresses = Addresses.Where(o => o.Location.Equals(LocationType.Home));
            if (homeAddresses != null && homeAddresses.Count() > 0)
            {
                homeAddresses.ToList().ForEach(o => Addresses.Remove(o));
            }
            Addresses.Add(new Address
            {
                Line1 = line1,
                Line2 = line2,
                City = city,
                Pincode = pincode,
                State = state,
                Country = country,
                Status = AddressStatus.Active,
                Location = LocationType.Home
            });
        }

        internal void SetPrimaryEMail(string email)
        {
            var emails = Contacts.Where(o => o.ContactType.Equals(ContactTypeEnum.Email) && o.Primary.Equals(true));
            if (emails != null && emails.Count() > 0)
            {
                emails.ToList().ForEach(o => Contacts.Remove(o));
            }
            Contacts.Add(new Contact
            {
                ContactType = ContactTypeEnum.Email,
                Primary = true,
                Status = ContactStatus.Active,
                Location = LocationType.Personal,
                Info = email
            });
            Email = email;
        }
        internal void SetInstituteEmail(string email)
        {
            // var emails = Contacts.Where(o => o.ContactType.Equals(ContactTypeEnum.Email) && o.Primary.Equals(true));
            // if (emails != null && emails.Count() > 0)
            // {
            //     emails.ToList().ForEach(o => Contacts.Remove(o));
            // }
            Contacts.Add(new Contact
            {
                ContactType = ContactTypeEnum.Email,
                Primary = true,
                Status = ContactStatus.Active,
                Location = LocationType.Institute,
                Info = email
            });
            //Email = email;
        }

        public void SetHomePhone(string phone)
        {
            var phones =
                Contacts.Where(o => o.ContactType.Equals(ContactTypeEnum.Phone) && o.Location.Equals(LocationType.Home));
            if (phones != null && phones.Count() > 0)
            {
                phones.ToList().ForEach(o => Contacts.Remove(o));
            }
            Contacts.Add(new Contact
            {
                ContactType = ContactTypeEnum.Phone,
                Primary = false,
                Status = ContactStatus.Active,
                Location = LocationType.Home,
                Info = phone
            });
        }

        public void SetOccupation(string title, string particulars, DateTime? asOn)
        {
            var occupation = Occupations.Where(o => o.AsOn.Equals(asOn)).FirstOrDefault();
            if(occupation == null)
            {
                Occupations.Add(new Occupation {
                    Title = title,
                    Particulars = particulars,
                    AsOn = asOn.HasValue ? asOn.Value : DateTime.UtcNow
                });
            }
        }

        public string GetPrimaryEmail()
        {
            var contact =
                Contacts.Where(o => o.ContactType.Equals(ContactTypeEnum.Email) && o.Primary.Equals(true))
                    .FirstOrDefault();
            var email = contact==null || string.IsNullOrEmpty(contact.Info) ? this.Email : contact.Info;
            return email;
        }

        public string GetStudentInstituteEmail()
        {
            var contact =
                Contacts.Where(o => o.ContactType.Equals(ContactTypeEnum.Email) && o.Primary.Equals(true) && o.Location.Equals(LocationType.Institute))
                    .FirstOrDefault();
            var email = contact==null || string.IsNullOrEmpty(contact.Info) ? this.Email : contact.Info;
            return email;
        }

        internal void SetPrimaryMobile(string mobile)
        {
            var mobiles = Contacts.Where(o => o.ContactType.Equals(ContactTypeEnum.Mobile) && o.Primary.Equals(true));
            if (mobiles != null && mobiles.Count() > 0)
            {
                mobiles.ToList().ForEach(o => Contacts.Remove(o));
            }
            Contacts.Add(new Contact
            {
                ContactType = ContactTypeEnum.Mobile,
                Primary = true,
                Status = ContactStatus.Active,
                Location = LocationType.Personal,
                Info = mobile
            });
            Mobile = mobile;
        }

        public string GetPrimaryMobile()
        {
            var contact =
                Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile && o.Primary.Equals(true) && o.Status == ContactStatus.Active).FirstOrDefault();
            var mobile = contact==null || string.IsNullOrEmpty(contact.Info) ? this.Mobile : contact.Info;
            return mobile;
        }

        public string GetHomePhone()
        {
            var contact =
                Contacts.Where(o => o.ContactType == ContactTypeEnum.Phone && o.Location.Equals(LocationType.Home))
                    .FirstOrDefault();
            return contact == null ? string.Empty : contact.Info;
        }

        public string GetHomeAddress()
        {
            var addr = string.Empty;
            var ha = Addresses.Where(o => o.Location.Equals(LocationType.Home)).SingleOrDefault();
            if (ha != null)
            {
                addr = ha.ToString();
            }
            return addr.Trim();
        }

        public static Person Create(string name, DateTime? dob, GenderEnum? gender, string email, string mobile)
        {
            //TBD: Validations
            var p = new Person();
            //string fName,string mName,string lName,
            //p.FirstName = fName;
            //p.MiddleName = mName;
            //p.LastName = lName;
            p.Name = name;
            p.DOB = dob;
            p.Gender = gender;

            p.Contacts.Add(new Contact
            {
                Primary = true,
                Location = LocationType.Personal,
                ContactType = ContactTypeEnum.Email,
                Info = email
            });
            p.Contacts.Add(new Contact
            {
                Primary = true,
                Location = LocationType.Personal,
                ContactType = ContactTypeEnum.Mobile,
                Info = mobile
            });

            return p;
        }

        public static Person Create(string name)
        {
            var p = new Person();
            p.Name = name;
            p.GuessGender();
            p.DOB = new DateTime(1947, 8, 15);
            p.Contacts.Add(new Contact
            {
                Primary = true,
                Location = LocationType.Personal,
                ContactType = ContactTypeEnum.Email,
                Info = ""
            });
            p.Contacts.Add(new Contact
            {
                Primary = true,
                Location = LocationType.Personal,
                ContactType = ContactTypeEnum.Mobile,
                Info = ""
            });

            return p;
        }

        public static Person Create(string name, DateTime? dob, GenderEnum? gender)
        {
            //TBD: Validations
            var p = new Person();
            p.Name = name;
            p.DOB = dob;
            p.Gender = gender;
            p.Contacts.Add(new Contact
            {
                Primary = true,
                Location = LocationType.Personal,
                ContactType = ContactTypeEnum.Email,
                Info = ""
            });
            p.Contacts.Add(new Contact
            {
                Primary = true,
                Location = LocationType.Personal,
                ContactType = ContactTypeEnum.Mobile,
                Info = ""
            });
            return p;
        }

        public void GuessGender(string val = "m")
        {
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrWhiteSpace(FirstName)) return;
            Gender = GenderEnum.Male;
            var fem = FirstName.EndsWith("a", StringComparison.InvariantCultureIgnoreCase)
                      || FirstName.EndsWith("i", StringComparison.InvariantCultureIgnoreCase)
                      || FirstName.EndsWith("ee", StringComparison.InvariantCultureIgnoreCase);
            if (fem) Gender = GenderEnum.Female;
            if (val != "m")
            {
                val = val.Trim().ToLower();
                if (val.StartsWith("f") || val.StartsWith("g") || val.StartsWith("miss") || val.StartsWith("mrs") ||
                    val.StartsWith("0"))
                    Gender = GenderEnum.Female;
            }
        }
    }
}
