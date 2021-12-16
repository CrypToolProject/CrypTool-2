using CrypTool.CertificateLibrary.Certificates;
using CrypTool.Util.Logging;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections;

namespace CrypTool.CertificateLibrary.Util
{
    public class DNWrapper
    {

        #region Constructors

        /// <summary>
        /// Creates a new DNWrapper object.
        /// This will usually be used for CA certificates
        /// </summary>
        /// <param name="commonName"></param>
        /// <param name="organisation"></param>
        /// <param name="organisationalUnit"></param>
        /// <param name="country"></param>
        /// <param name="emailAddress"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DNWrapper(string commonName, string organisation, string organisationalUnit, string country, string emailAddress)
        {
            if (commonName == null)
            {
                throw new ArgumentNullException("commonName");
            }
            if (organisation == null)
            {
                throw new ArgumentNullException("organisation");
            }
            if (organisationalUnit == null)
            {
                throw new ArgumentNullException("organisationalUnit");
            }
            if (country == null)
            {
                throw new ArgumentNullException("country");
            }
            if (emailAddress == null)
            {
                throw new ArgumentNullException("emailAddress");
            }
            CommonName = commonName;
            Organisation = organisation;
            OrganisationalUnit = organisationalUnit;
            Country = country;
            EmailAddress = emailAddress;
        }

        /// <summary>
        /// Creates a new DNWrapper object just filling out the avatar as CommonName.
        /// This will usually be used for peer certificates.
        /// </summary>
        /// <param name="avatarName">The avatar name, which will be stored as CommonName</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DNWrapper(string avatarName)
            : this(avatarName, string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Creates a new DNWrapper object by using a BouncyCastle X509Name Object
        /// </summary>
        /// <param name="avatarName">The BouncyCastle X509Name</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="X509CertificateFormatException">Certificates distinguished name is empty or malformed</exception>
        public DNWrapper(X509Name dn)
        {
            if (dn == null)
            {
                throw new ArgumentNullException("dn", "Distinguished name cannot be null");
            }

            // Read the CN, O, OU, C, EmailAddress values from Destinguished Name
            IList values = dn.GetValueList();
            IList oids = dn.GetOidList();
            if (values.Count != oids.Count)
            {
                // Is thrown when the certificate was created in a very strange way ;)
                string msg = "Error while loading certificate: Error in subject distinguish name: \nNumber of DerObjectIdentifiers and values differ.";
                Log.Error(msg);
                throw new X509CertificateFormatException(msg);
            }

            CommonName = string.Empty;
            Organisation = string.Empty;
            OrganisationalUnit = string.Empty;
            Country = string.Empty;
            EmailAddress = string.Empty;

            if (oids.Count == 0)
            {
                string msg = "Loading CA Certificate: Certificate disregards subject distinguished name as defined by RFCs.";
                Log.Warn(msg);
                throw new X509CertificateFormatException(msg);
            }

            // Save the subjectDN values in this object to easily access them.
            for (int i = 0; i < oids.Count; i++)
            {
                DerObjectIdentifier id = (DerObjectIdentifier)oids[i];
                if (id.Equals(X509Name.CN))
                {
                    CommonName = values[i].ToString();
                }
                else if (id.Equals(X509Name.O))
                {
                    Organisation = values[i].ToString();
                }
                else if (id.Equals(X509Name.OU))
                {
                    OrganisationalUnit = values[i].ToString();
                }
                else if (id.Equals(X509Name.C))
                {
                    Country = values[i].ToString();
                }
                else if (id.Equals(X509Name.EmailAddress))
                {
                    EmailAddress = values[i].ToString();
                }
            }
        }

        #endregion


        #region Access methods

        public X509Name GetDN()
        {
            Hashtable attrs = new Hashtable();
            ArrayList order = new ArrayList();

            attrs.Add(X509Name.CN, CommonName);
            attrs.Add(X509Name.O, Organisation);
            attrs.Add(X509Name.OU, OrganisationalUnit);
            attrs.Add(X509Name.C, Country);
            attrs.Add(X509Name.EmailAddress, EmailAddress);

            order.Add(X509Name.CN);
            order.Add(X509Name.O);
            order.Add(X509Name.OU);
            order.Add(X509Name.C);
            order.Add(X509Name.EmailAddress);

            return new X509Name(order, attrs);
        }

        public override string ToString()
        {
            return GetDN().ToString();
        }

        #endregion


        #region Properties

        public string CommonName { get; set; }

        public string Organisation { get; set; }

        public string OrganisationalUnit { get; set; }

        public string Country { get; set; }

        public string EmailAddress { get; set; }

        #endregion

    }
}