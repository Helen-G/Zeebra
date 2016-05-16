using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.TestDoubles;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    public class TransferFundValidationTests : AdminWebsiteUnitTestsBase
    {
        private Core.Brand.Data.Brand _brand;
        private Guid _playerId;
        private Guid _productWalletId;

        private ITransferFundValidationService _validationService;
        private FakePaymentRepository _repository;
        private PaymentTestHelper _paymentTestHelper;
        private TransferFundTestHelper _transferFundTestHelper;

        private double _previousTransferDaysShift;

        public override void BeforeEach()
        {
            base.BeforeEach();

            var brandTestHelper = Container.Resolve<BrandTestHelper>();
            Container.Resolve<SecurityTestHelper>().SignInUser();
            _brand = brandTestHelper.CreateBrand(isActive: true);

            _playerId = Container.Resolve<PlayerTestHelper>().CreatePlayer(null, true, _brand.Id);
            _productWalletId = _brand.WalletTemplates.Single(x => !x.IsMain).Id;
            _validationService = Container.Resolve<ITransferFundValidationService>();
            _repository = Container.Resolve<FakePaymentRepository>();

            _paymentTestHelper = Container.Resolve<PaymentTestHelper>();
            _transferFundTestHelper = Container.Resolve<TransferFundTestHelper>();
        }

        [Test]
        public void Should_throw_exception_when_required_parameters_missed_in_request()
        {
            // Arrange
            var transferFundRequest = new TransferFundRequest();

            Action action = () =>
            {
                _validationService.Validate(transferFundRequest);
            };

            //Act
            action.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Not_validate_if_Wallet_not_exist()
        {
            // Arrange
            var transferFundRequest = new TransferFundRequest
            {
                PlayerId = _playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = new Guid("00000000-0000-0000-0000-000000000000").ToString()
            };

            //Act
            _validationService.Validate(transferFundRequest).ErrorMessage
                .Should().BeEquivalentTo("Wallet with id 00000000-0000-0000-0000-000000000000 not found.");
        }

        [Test]
        public void Not_validate_if_zero_or_negative_Amount_in_request()
        {
            // Arrange
            var transferFundRequest = new TransferFundRequest
            {
                PlayerId = _playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = _productWalletId.ToString()
            };

            //Act
            _validationService.Validate(transferFundRequest).ErrorMessage
                .Should().BeEquivalentTo("app:payment.amountMustBeGreaterThanZero");
        }

        [Test]
        public void Not_validate_if_Amount_greater_than_MainBalance()
        {
            // Arrange
            var transferFundRequest = new TransferFundRequest
            {
                Amount = 1,
                PlayerId = _playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = _productWalletId.ToString()
            };

            //Act
            _validationService.Validate(transferFundRequest).ErrorMessage
                .Should().BeEquivalentTo("app:payment.amountExceedsBalance");
        }

        [Test]
        public void Validate_if_Amount_less_or_equal_than_MainBalance()
        {
            // Arrange
            _paymentTestHelper.MakeDeposit(_playerId, 1);

            var transferFundRequest = new TransferFundRequest
            {
                Amount = 1,
                PlayerId = _playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = _productWalletId.ToString()
            };

            //Act
            _validationService.Validate(transferFundRequest).IsValid
                .Should().BeTrue();
        }

        [Test]
        public void Not_validate_if_Amount_less_than_MinAmountPerTransaction()
        {
            // Arrange
            _paymentTestHelper.MakeDeposit(_playerId, 10);

            var transferSettings = new SaveTransferSettingsCommand {MinAmountPerTransaction = 2};
            _transferFundTestHelper.CreateTransferSettings(_brand, transferSettings);

            var transferFundRequest = new TransferFundRequest
            {
                Amount = 1,
                PlayerId = _playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = _productWalletId.ToString()
            };

            //Act
            _validationService.Validate(transferFundRequest).ErrorMessage
                .Should().Contain("amount is below the allowed value");
        }

        [Test]
        public void Not_validate_if_Amount_greater_than_MaxAmountPerTransaction()
        {
            // Arrange
            _paymentTestHelper.MakeDeposit(_playerId, 10);

            var transferSettings = new SaveTransferSettingsCommand { MaxAmountPerTransaction = 1 };
            _transferFundTestHelper.CreateTransferSettings(_brand, transferSettings);

            var transferFundRequest = new TransferFundRequest
            {
                Amount = 2,
                PlayerId = _playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = _productWalletId.ToString()
            };

            //Act
            _validationService.Validate(transferFundRequest).ErrorMessage
                .Should().Contain("amount exceeds the allowed value");
        }

        [Test]
        public void Not_validate_if_Amount_greater_than_MaxAmountPerDay()
        {
            // Arrange
            _paymentTestHelper.MakeDeposit(_playerId, 10);

            var transferSettings = new SaveTransferSettingsCommand {MaxAmountPerDay = 1};
            _transferFundTestHelper.CreateTransferSettings(_brand, transferSettings);

            var transferFundRequest = new TransferFundRequest
            {
                Amount = 2,
                PlayerId = _playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = _productWalletId.ToString()
            };

            //Act
            _validationService.Validate(transferFundRequest).ErrorMessage
                .Should().Contain("amounts exceed the daily limit");
        }

        [Test]
        public void Not_validate_if_transfer_quantity_perDay_greater_than_MaxTransactionPerDay()
        {
            // Arrange
            _paymentTestHelper.MakeDeposit(_playerId, 10);
            _previousTransferDaysShift = -0.5;

            _transferFundTestHelper.AddTransfer(_brand, _playerId, _repository, _previousTransferDaysShift);
            var transferSettings = new SaveTransferSettingsCommand { MaxTransactionPerDay = 1 };
            _transferFundTestHelper.CreateTransferSettings(_brand, transferSettings);

            var transferFundRequest = new TransferFundRequest
            {
                Amount = 1,
                PlayerId = _playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = _productWalletId.ToString()
            };

            //Act
            _validationService.Validate(transferFundRequest).ErrorMessage
                .Should().Contain("transfers per day exceed daily limit");
        }

        [Test]
        public void Validate_if_transfer_quantity_perDay_less_than_MaxTransactionPerDay()
        {
            // Arrange
            _paymentTestHelper.MakeDeposit(_playerId, 10);
            _previousTransferDaysShift = -2;

            _transferFundTestHelper.AddTransfer(_brand, _playerId, _repository, _previousTransferDaysShift);
            var transferSettings = new SaveTransferSettingsCommand { MaxTransactionPerDay = 1 };
            _transferFundTestHelper.CreateTransferSettings(_brand, transferSettings);

            var transferFundRequest = new TransferFundRequest
            {
                Amount = 1,
                PlayerId = _playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = _productWalletId.ToString()
            };

            //Act
            _validationService.Validate(transferFundRequest).IsValid
                .Should().BeTrue();
        }

        [Test]
        public void Validate_if_transfer_quantity_perWeek_less_than_MaxTransactionPerWeek()
        {
            // Arrange
            _paymentTestHelper.MakeDeposit(_playerId, 10);
            _previousTransferDaysShift = -8;

            _transferFundTestHelper.AddTransfer(_brand, _playerId, _repository, _previousTransferDaysShift);
            var transferSettings = new SaveTransferSettingsCommand { MaxTransactionPerWeek = 1 };
            _transferFundTestHelper.CreateTransferSettings(_brand, transferSettings);

            var transferFundRequest = new TransferFundRequest
            {
                Amount = 1,
                PlayerId = _playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = _productWalletId.ToString()
            };

            //Act
            _validationService.Validate(transferFundRequest).IsValid
                .Should().BeTrue();
        }

        [Test]
        public void Not_validate_if_transfer_quantity_perWeek_greater_than_MaxTransactionPerWeek()
        {
            // Arrange
            _paymentTestHelper.MakeDeposit(_playerId, 10);
            _previousTransferDaysShift = -6;

            _transferFundTestHelper.AddTransfer(_brand, _playerId, _repository, _previousTransferDaysShift);
            var transferSettings = new SaveTransferSettingsCommand { MaxTransactionPerWeek = 1 };
            _transferFundTestHelper.CreateTransferSettings(_brand, transferSettings);

            var transferFundRequest = new TransferFundRequest
            {
                Amount = 1,
                PlayerId = _playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = _productWalletId.ToString()
            };

            //Act
            _validationService.Validate(transferFundRequest).ErrorMessage
                .Should().Contain("transfers per day exceed weekly limit");
        }

        [Test]
        public void Validate_if_transfer_quantity_perMonth_less_than_MaxTransactionPerMonth()
        {
            // Arrange
            _paymentTestHelper.MakeDeposit(_playerId, 10);
            _previousTransferDaysShift = -31;

            _transferFundTestHelper.AddTransfer(_brand, _playerId, _repository, _previousTransferDaysShift);
            var transferSettings = new SaveTransferSettingsCommand { MaxTransactionPerMonth = 1 };
            _transferFundTestHelper.CreateTransferSettings(_brand, transferSettings);

            var transferFundRequest = new TransferFundRequest
            {
                Amount = 1,
                PlayerId = _playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = _productWalletId.ToString()
            };

            //Act
            _validationService.Validate(transferFundRequest).IsValid
                .Should().BeTrue();
        }

        [Test]
        public void Not_validate_if_transfer_quantity_perMonth_greater_than_MaxTransactionPerMonth()
        {
            // Arrange
            _paymentTestHelper.MakeDeposit(_playerId, 10);
            _previousTransferDaysShift = -29;

            _transferFundTestHelper.AddTransfer(_brand, _playerId, _repository, _previousTransferDaysShift);
            var transferSettings = new SaveTransferSettingsCommand { MaxTransactionPerMonth = 1 };
            _transferFundTestHelper.CreateTransferSettings(_brand, transferSettings);

            var transferFundRequest = new TransferFundRequest
            {
                Amount = 1,
                PlayerId = _playerId,
                TransferType = TransferFundType.FundIn,
                WalletId = _productWalletId.ToString()
            };

            //Act
            _validationService.Validate(transferFundRequest).ErrorMessage
                .Should().Contain("transfers per day exceed monthly limit");
        }
    }
}
