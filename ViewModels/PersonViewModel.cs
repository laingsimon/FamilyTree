using System;
using System.Text;
using System.Web.Mvc;
using FamilyTree.Models;
using FamilyTree.Models.DTO;

namespace FamilyTree.ViewModels
{
    public class PersonViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        // ReSharper disable MemberCanBePrivate.Global
        public string MiddleName { get; set; }
        public string Title { get; set; }
        public string Nickname { get; set; }
        public Gender Gender { get; set; }
        // ReSharper restore MemberCanBePrivate.Global
        public bool HasOtherTree { get; set; }

        public EventViewModel Birth { get; set; }
        public EventViewModel Death { get; set; }

        /// <summary>
        /// Children born out of wed-lock
        /// </summary>
        public PersonViewModel[] Children { get; set; }

        /// <summary>
        /// Marriages of this person to another
        /// </summary>
        public MarriageViewModel[] Marriages { get; set; }

        public string GetHandle(bool useRawBirthDate = false)
        {
            var fullName = _FullName(false).Replace(" ", "-");
            var birthDate = _BirthDate(useRawBirthDate);

            if (string.IsNullOrEmpty(birthDate))
                return fullName;

            return fullName + "_" + birthDate.Replace(" ", "");
        }

        private string _BirthDate(bool useRawBirthDate = false)
        {
            if (Birth == null)
                return null;

            if (useRawBirthDate)
            {
                var rawDate = Birth.RawDate ?? "";
                return rawDate.Replace("/", "");
            }

            if (string.IsNullOrEmpty(Birth.DateFormatted))
                return null;

            return Birth.DateFormatted.Replace("/", "");
        }

        private string _FullName(bool includeNick = true)
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(Title))
                builder.Append(Title + " ");

            builder.Append(FirstName);

            if (includeNick && !string.IsNullOrEmpty(Nickname))
                builder.AppendFormat(" '{0}'", Nickname);

            builder.Append(" ");

            if (!string.IsNullOrEmpty(MiddleName))
                builder.Append(MiddleName + " ");

            builder.Append(LastName);

            return builder.ToString();
        }

        public string GetPhotoUri(UrlHelper url, int? height = null, int? width = null)
        {
            var dob = Birth != null && Birth.Date.HasValue
                ? Birth.Date.Value.ToString("ddMMyyyy").ToBase64()
                : "-";

            return url.Action("Index", "Photo", new
            {
                family = LastName != null ? LastName.Replace("?", "") : null,
                firstName = FirstName != null ? FirstName.Replace("?", "") : null,
                middleName = MiddleName != null ? MiddleName.Replace("?", "") : null,
                dob,
                size = _GetSize(width, height)
            }, "http");
        }

        private static string _GetSize(int? width, int? height)
        {
            if (width.HasValue && height.HasValue)
                return string.Format("{0}x{1}", width.Value, height.Value);

            if (width.HasValue)
                return string.Format("w{0}", width.Value);

            if (height.HasValue)
                return string.Format("h{0}", height.Value);

            return "";
        }

        public string GenderCss
        {
            get { return Gender.ToString().ToLower(); }
        }

        public bool IsForAnotherFamily(UrlHelper url)
        {
            var requestedFamily = (string)url.RequestContext.RouteData.Values["family"];
            return !requestedFamily.Equals(LastName, StringComparison.OrdinalIgnoreCase);
        }

        public string GetUrl(UrlHelper url)
        {
            return url.Action("Family", "Tree", new { family = LastName }) + "#" + GetHandle();
        }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(Nickname))
                    return string.Format("{0} '{1}' {2}", FirstName, Nickname, LastName);

                return string.Format("{0} {1}", FirstName, LastName);
            }
        }
    }
}