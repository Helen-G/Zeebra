define([], function() {
	function GameLimit(currencies, gameProviders) {
		var self = this;
		self.id = ko.observable();
		self.brandId = ko.observable();
		self.currencies = ko.observableArray(currencies);
		self.selectedCurrency = ko.observable();

		self.betLimits = ko.observableArray();
		self.selectedBetLimit = ko.observable();

		self.gameProviders = ko.observableArray(gameProviders());
		self.selectedGameProvider = ko.observable();

		self.selectedGameProvider.subscribe(function(newGame) {
			if (newGame === null ||!newGame) {
				return;
			}
			self.betLimits([]);
			$.ajax("game/BetLimits?gameProviderId=" + newGame.id + "&brandId="+self.brandId(), {async: false}).done(function (response) {
				self.betLimits(response.betLimits);
			});
		});

		self.canChangeCurrency = ko.observable(true);

		self.selectedCurrencyCode = ko.computed(function() {
			var selectedCurrency = self.selectedCurrency();
			var selectedCurrencyCode = selectedCurrency ? selectedCurrency.code : "";

			return selectedCurrencyCode;
		});
		
		self.selectedBetLimitId = ko.computed(function () {
			var betLimit = self.selectedBetLimit();
			var limitId = betLimit ? betLimit.limitId : "";

			return limitId;
		});
		
		self.selectedGameProviderName = ko.computed(function () {
			var selectedGame = self.selectedGameProvider();
			var selectedGameName = selectedGame ? selectedGame.name : "";

			return selectedGameName;
		});

		self.setBetLimitById = function(id) {
			return ko.utils.arrayForEach(self.betLimits(), function (betLimit) {
				if (betLimit.id === id) {
					self.selectedBetLimit(betLimit);
					return;
				}
			});
		};
		
		self.setGameProviderById = function (id) {
			return ko.utils.arrayForEach(self.gameProviders(), function (gameProvider) {
			    if (gameProvider.id === id) {
			        self.selectedGameProvider(gameProvider);
					return;
				}
			});
		};

		self.setCurrencyCode = function (code) {
			return ko.utils.arrayForEach(self.currencies(), function (currency) {
				if (currency.code === code) {
					self.selectedCurrency(currency);
					return;
				}
			});
		};
	};

	return GameLimit;
})