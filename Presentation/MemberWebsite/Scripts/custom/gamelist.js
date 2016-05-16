var GameListModel = function () {
    var self = this;

    // For tab panels
    self.shownTab = ko.observable('tabContent1');
    self.toggleTab = function(){
        var target = event.target.hash.substr(1);
        self.shownTab(target);
    };

    // end--
};

ko.applyBindings(new GameListModel(), document.getElementById("game-list-wrapper"));