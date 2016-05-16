using System;
using System.Linq;
using System.Linq.Expressions;
using AFT.RegoV2.MemberApi.Interface;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.Tests.Common;
using NUnit.Framework;


namespace AFT.RegoV2.Tests.Unit.Player
{
    using System.Threading.Tasks;
    using TestPair = Pair<Expression<Func<RegisterRequest, object>>, object>;

    internal class RegistrationTests : PlayerServiceTestsBase
    {
        [Test]
        public async Task Can_register_Player()
        {
            var registrationData = TestDataGenerator.CreateRandomRegistrationRequestData();

            await PlayerWebservice.RegisterAsync(registrationData);

        }

        public static TestPair[] GetRequiredFields()
        {
            return new[]
            {
                new TestPair(r => r.Username, null),
                new TestPair(r => r.Password, null),
                new TestPair(r => r.FirstName, null),
                new TestPair(r => r.LastName, null),
                new TestPair(r => r.MailingAddressLine1, null),
                new TestPair(r => r.MailingAddressPostalCode, null),
                new TestPair(r => r.CountryCode, null),
                new TestPair(r => r.CurrencyCode, null),
                new TestPair(r => r.Email, null),
                new TestPair(r => r.PhoneNumber, null),
                new TestPair(r => r.DateOfBirth, null),
                new TestPair(r => r.Gender, null),
                new TestPair(r => r.Title, null),
                new TestPair(r => r.MailingAddressCity, null),
                new TestPair(r => r.ContactPreference, null),
                new TestPair(r => r.Username, string.Empty),
                new TestPair(r => r.Password, string.Empty),
                new TestPair(r => r.FirstName, string.Empty),
                new TestPair(r => r.LastName, string.Empty),
                new TestPair(r => r.MailingAddressLine1, string.Empty),
                new TestPair(r => r.MailingAddressPostalCode, string.Empty),
                new TestPair(r => r.CountryCode, string.Empty),
                new TestPair(r => r.CurrencyCode, string.Empty),
                new TestPair(r => r.Email, string.Empty),
                new TestPair(r => r.PhoneNumber, string.Empty),
                new TestPair(r => r.DateOfBirth, string.Empty),
                new TestPair(r => r.Gender, string.Empty),
                new TestPair(r => r.Title, string.Empty),
                new TestPair(r => r.MailingAddressCity, string.Empty),
                new TestPair(r => r.ContactPreference, string.Empty),
                new TestPair( r=> r.SecurityQuestionId, string.Empty),
                new TestPair( r => r.SecurityAnswer, string.Empty),
            };
        }

        public static TestPair[] GetAllowedRangeFields()
        {
            var str = string.Empty;
            return new[]
            {
                new TestPair(r => r.Username, str.PadRight(5, 'a')),
                new TestPair(r => r.Username, str.PadRight(13, 'b')),

/*                new TestPair(r => r.Username, "qwerty-"),
                new TestPair(r => r.Username, "-qwerty"),
                new TestPair(r => r.Username, "q--werty"),
                new TestPair(r => r.Username, "qwerty'"),
                new TestPair(r => r.Username, "'qwerty"),
                new TestPair(r => r.Username, "qw''erty"),
                new TestPair(r => r.Username, "qwerty_"),
                new TestPair(r => r.Username, "_qwerty"),
                new TestPair(r => r.Username, "qwe__rty"),
                new TestPair(r => r.Username, "qwerty."),
                new TestPair(r => r.Username, ".qwerty"),
                new TestPair(r => r.Username, "qwe..rty"),*/

                new TestPair(r => r.Email, "@test.com".PadLeft(51, 'c')),
                new TestPair(r => r.Email, "ewrewerwerwerwer"),

                new TestPair(r => r.Password, str.PadRight(5, 'd')),
                new TestPair(r => r.Password, str.PadRight(13, 'd')),

                new TestPair(r => r.FirstName, str.PadLeft(51, 'e')),
                new TestPair(r => r.FirstName, "werwerwerwerw  werwerw"),
                new TestPair(r => r.FirstName, " werwerwerwerw werwerw"),
                new TestPair(r => r.FirstName, "werwerwerwerw werwerw "),
                /*new TestPair(r => r.FirstName, "werwerwerwerw_"),*/
                new TestPair(r => r.FirstName, "werwerwerwerw+"),

                /*new TestPair(r => r.FirstName, "qwerty-"),
                new TestPair(r => r.FirstName, "-qwerty"),
                new TestPair(r => r.FirstName, "q--werty"),
                new TestPair(r => r.FirstName, "qwerty'"),
                new TestPair(r => r.FirstName, "'qwerty"),
                new TestPair(r => r.FirstName, "qw''erty"),
                new TestPair(r => r.FirstName, "qwerty."),
                new TestPair(r => r.FirstName, ".qwerty"),
                new TestPair(r => r.FirstName, "qwe..rty"),*/

                new TestPair(r => r.LastName, str.PadLeft(21, 'f')),
                new TestPair(r => r.LastName, "werweerwerw  werwer"),
                new TestPair(r => r.LastName, " werwerrwerw werwerw"),
                new TestPair(r => r.LastName, "werrwerwerw werwerw "),
                /*new TestPair(r => r.LastName, "werwerwerwerw_"),*/
                new TestPair(r => r.LastName, "werwerwerwerw+"),

                /*new TestPair(r => r.LastName, "qwerty-"),
                new TestPair(r => r.LastName, "-qwerty"),
                new TestPair(r => r.LastName, "q--werty"),
                new TestPair(r => r.LastName, "qwerty'"),
                new TestPair(r => r.LastName, "'qwerty"),
                new TestPair(r => r.LastName, "qw''erty"),
                new TestPair(r => r.LastName, "qwerty."),
                new TestPair(r => r.LastName, ".qwerty"),
                new TestPair(r => r.LastName, "qwe..rty"),*/

                new TestPair(r => r.PhoneNumber, str.PadRight(7, '1')),
                new TestPair(r => r.PhoneNumber, str.PadRight(16, '1')),
                new TestPair(r => r.PhoneNumber, "111 222 11 22"),

                new TestPair(r => r.DateOfBirth, "111 222 11 22"),
                new TestPair(r => r.DateOfBirth, "01/13/2014"),
                new TestPair(r => r.DateOfBirth, "12/12/2014"),

                new TestPair(r => r.MailingAddressLine1, str.PadLeft(51, 'g')),
                new TestPair(r => r.CountryCode, "RU"),
                new TestPair(r => r.CurrencyCode, "RUB"),

                new TestPair(r => r.Email, "Abc.example.com"),
                new TestPair(r => r.Email, "A@b@c@example.com"),
                
                new TestPair(r => r.Email, @"a""b(c)d,e:f;g<h>i[j\k]l@example.com"),
                new TestPair(r => r.Email, @"just""not""right@example.com"),
                new TestPair(r => r.Email, @"this is""not\allowed@example.com"),
                new TestPair(r => r.Email, @"this\ still\""notallowed@example.com"),

                new TestPair(r => r.MailingAddressPostalCode, str),
                new TestPair(r => r.MailingAddressPostalCode, str.PadRight(11, '1')),
                //new TestPair(r => r.BrandId, new Guid("00000000-0000-0000-0000-000000000001")),

                new TestPair(r => r.MailingAddressCity, str),
                new TestPair(r => r.ContactPreference, str),

                new TestPair(r => r.SecurityQuestionId, str),
                new TestPair( r => r.SecurityAnswer, str),

            };
        }

        public static TestPair[] GetValidCases()
        {
            var str = string.Empty;
            return new[]
            {
                new TestPair(r => r.Username, str.PadRight(6, 'a')),

                new TestPair(r => r.Username, "q-wert-y"),
                new TestPair(r => r.Username, "qw'er'ty"),
                new TestPair(r => r.Username, "q_we_rty"),
                new TestPair(r => r.Username, "qwe.rt.y"),

                new TestPair(r => r.Email, "test@test.com"),

                new TestPair(r => r.Password, str.PadRight(6, 'd')),

                new TestPair(r => r.FirstName, str.PadLeft(5, 'e')),

                new TestPair(r => r.FirstName, "q-wert-y"),
                new TestPair(r => r.FirstName, "qw'er'ty"),
                new TestPair(r => r.FirstName, "qwe.rt.y"),
/*                new TestPair(r => r.FirstName, "qwe rty"),
                new TestPair(r => r.FirstName, "q we rty"),*/

                new TestPair(r => r.LastName, str.PadLeft(5, 'f')),

                new TestPair(r => r.LastName, "q-wert-y"),
                new TestPair(r => r.LastName, "qw'er'ty"),
                new TestPair(r => r.LastName, "qwe.rt.y"),
/*                new TestPair(r => r.LastName, "qwe rty"),
                new TestPair(r => r.LastName, "q we rty"),*/

                new TestPair(r => r.MailingAddressPostalCode, "ret 33"),
                new TestPair(r => r.MailingAddressLine1, "ret 33 444 433"),

                new TestPair(r => r.Email, "niceandsimple@example.com"),
                new TestPair(r => r.Email, "very.common@example.com"),
                new TestPair(r => r.Email, "a.little.lengthy.but.fine@dept.example.com"),
                new TestPair(r => r.Email, "disposable.style.email.with+symbol@example.com"),
                new TestPair(r => r.Email, "other.email-with-dash@example.com"),
                new TestPair(r => r.Email, "!#$%&'*+-/=?^_`{}|~@example.org"),

                new TestPair(r => r.Gender, "Male"),
                new TestPair( r=> r.Title, "Mr"),
                new TestPair( r => r.MailingAddressLine2, "address line 2"),
                new TestPair( r => r.MailingAddressLine3, "address line 3"),
                new TestPair( r => r.MailingAddressLine4, "address line 4"),
                new TestPair( r => r.MailingAddressCity, "Singapore"),
                new TestPair( r => r.ContactPreference, "Email"),
                /*
                new TestPair(r => r.Email, "\"\"much.more unusual\"\"@example.com"),
                new TestPair(r => r.Email, "\"\"very.unusual.@.unusual.com\"\"@example.com"),
                new TestPair(r => r.Email, "\"\"very.(),:;<>[]\"\".VERY.\"\"very@\"\"very\"\".unusual\"\"@strange.example.com"),
                new TestPair(r => r.Email, "postbox@com"),
                new TestPair(r => r.Email, "admin@mailserver1"),
                new TestPair(r => r.Email, "\"\"()<>[]:,;@\\\"\"!#$%&'*+-/=?^_`{}| ~.a\"\"@example.org"),
                new TestPair(r => r.Email, "\"\"-\"\"@example.org"),
                new TestPair(r => r.Email, "üñîçøðé@example.com"),
                new TestPair(r => r.Email, "üñîçøðé@üñîçøðé.com"),
                */
            };
        }

        [Test]
        [TestCaseSource("GetRequiredFields", Category = "Unit")]
        public void Player_regisration_fails_when_required_field_is_missing(TestPair propertyValuePair)
        {
            ValidateTestCase(propertyValuePair);
        }

        [Test]
        [TestCaseSource("GetAllowedRangeFields", Category = "Unit")]
        public void Player_registration_fails_when_fields_not_within_its_allowed_range(TestPair propertyValuePair)
        {
            ValidateTestCase(propertyValuePair);
        }

        [Test]
        [TestCaseSource("GetValidCases", Category = "Unit")]
        public void Player_registration_should_not_fail_when_fields_are_valid(TestPair propertyValuePair)
        {
            ValidateTestCase(propertyValuePair, true);
        }

        [Test]
        public void Player_registration_fails_when_username_already_exists()
        {
            const string userName = "user_name";
            var brand139 = new Core.Brand.Data.Brand { Id = Guid.NewGuid(), Name = "139" };
            FakePlayerRepository.Players.Add(new Core.Player.Data.Player { Username = userName, BrandId = brand139.Id });

            var testCase = new TestPair(r => r.Username, userName);
            ValidateTestCase(testCase, true);

            FakeBrandRepository.Brands.Add(brand139);
            ValidateTestCase(testCase);
        }

        [Test]
        public void Player_registration_fails_when_email_already_exists()
        {
            const string email = "test@test.com";

            var brand139 = new Core.Brand.Data.Brand { Id = Guid.NewGuid(), Name = "139" };
            FakePlayerRepository.Players.Add(new Core.Player.Data.Player { Email = email, BrandId = brand139.Id });

            var testCase = new TestPair(r => r.Email, email);
            ValidateTestCase(testCase, true);

            FakeBrandRepository.Brands.Add(brand139);

            ValidateTestCase(testCase);
        }

        [Test]
        public void Player_regisration_fails_when_passwords_do_not_match()
        {
            ValidateTestCase(new TestPair(r => r.PasswordConfirm, TestDataGenerator.GetRandomString()));
        }

        private async void ValidateTestCase(TestPair propertyValuePair, bool validCase = false)
        {
            var propertyName = GetPropertyName(propertyValuePair.Item1);

            var registrationData = TestDataGenerator.CreateRandomRegistrationRequestData();

            SetPropertyValue(registrationData, propertyName, propertyValuePair.Item2);
            if (propertyName == "Password")
            {
                SetPropertyValue(registrationData, "PasswordConfirm", propertyValuePair.Item2);
            }

            if (validCase)
            {
                var result = await PlayerWebservice.RegisterAsync(registrationData);
                Assert.IsNotNull(result);
                Assert.That(result.UserId, Is.Not.EqualTo(Guid.Empty));
            }
            else
            {
                var e = Assert.Throws<MemberApiProxyException>(async () => await PlayerWebservice.RegisterAsync(registrationData)).Exception;
                Assert.That(e.ErrorMessage, Is.Not.Empty);
                Assert.That(e.Violations.Count(), Is.GreaterThanOrEqualTo(1));
                Assert.That(e.Violations.First().ErrorCode, Is.Not.Empty);
                Assert.That(e.Violations.First().ErrorMessage, Is.Not.Empty);
            }
        }

        #region Test helpers

        private static void SetPropertyValue(RegisterRequest registrationData, string fieldName, object emptyValue)
        {
            var property = registrationData.GetType().GetProperty(fieldName);
            property.SetValue(registrationData, emptyValue);
        }

        private static string GetPropertyName(Expression<Func<RegisterRequest, object>> propertyExpression)
        {
            return GetMemberInfo(propertyExpression).Member.Name;
        }

        private static MemberExpression GetMemberInfo(Expression method)
        {
            var lambda = method as LambdaExpression;
            if (lambda == null)
            {
                throw new ArgumentNullException("method");
            }

            MemberExpression memberExpr = null;

            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpr =
                    ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null)
            {
                throw new ArgumentException("method");
            }

            return memberExpr;
        }

        #endregion
    }

    [Serializable]
    public sealed class Pair<TKey, TValue> : Tuple<TKey, TValue>
    {
        public Pair(TKey first, TValue second) : base(first, second) { }

        public TKey Key
        {
            get { return Item1; }
        }

        public TValue Value
        {
            get { return Item2; }
        }
    }
}