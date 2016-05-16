define([], function () {
	function BetLimit(levelsList) {
		var self = this;
		self.bLevels = levelsList;
		self.id = ko.observable();
		self.name = ko.observable().extend({
			required: true, minLength: 1, maxLength: 50,
			pattern: {
				message: "Name can only contain alphanumeric characters.", params: "^[a-zA-Z-0-9 ]+$"
			}
		});
		self.code = ko.observable().extend({
			required: true, minLength: 1, maxLength: 20,
			pattern: {
				message: "Code can only contain alphanumeric characters.", params: "^[a-zA-Z-0-9]+$"
			}
		});
		self.description = ko.observable().extend({
			required: false,
			maxLength: 250
		});

		self.label = ko.observable();

		var labelNumber = self.bLevels().length > 0
            ? _.max(self.bLevels(), function (o) { return o.labelNumber; }).labelNumber + 1
            : 1;

	    self.labelNumber = labelNumber;

		self.label("Bet limit " + labelNumber);

		self.canRemove = ko.computed(function () {
			return self.bLevels.length > 1;
		});

		self.errors = ko.validation.group(this);
	};

	return BetLimit;
});