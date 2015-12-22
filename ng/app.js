'use strict';

$("#menu-toggle").click(function (e) {
    e.preventDefault();
    $("#wrapper").toggleClass("toggled");
});

// Declare app level module which depends on views, and components
angular.module('expApp', [
  'data-entry',
  'data-edit',
  'monthly-view',
  'search-view',
  'ngRoute',
  'checklist-model',
  'ngAnimate'
])
.config(['$routeProvider', '$locationProvider', function ($routeProvider, $locationProvider) {
    var now = new Date();
    var d = new Date(now.getFullYear(), now.getMonth() - 1, 1, 0, 0, 0);
    $routeProvider.otherwise({ redirectTo: '/monthly/' + d.getFullYear() + '/' + (d.getMonth() + 1) });
    $locationProvider.html5Mode(true);
}])
.controller('sideBarController', ['$rootScope', '$http', '$window', function ($rootScope, $http, $window) {
    var now = new Date();
    this.months = [];

    for (var i = 0; i < 6; i++) {
        var d = new Date(now.getFullYear(), now.getMonth() - i, 1, 0, 0, 0);
        this.months[i] = { year: d.getFullYear(), month: d.getMonth() };
    }

    $rootScope.state = {};
    $rootScope.updateCurrentMonth = function (year, month) {
        $rootScope.state.currentMonth = { year: year, month: month };
    };
    $rootScope.logout = function () {
        $http.get('/expensedata/logout')
        .then(function (resp) {
            $window.location.href = '/login';
        })
    };
}])
.filter('monthName', function () {
    return function (value) {
        return ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"][value];
    };
})
.filter('categoryName', ['$rootScope', function ($rootScope) {
	return function (value) {
		return $rootScope.expenseCategories[value];
	};
}])
.directive('summaryPanel', function () {
    return {
        restrict: 'AE',
        templateUrl: 'summary-panel.html',
        scope: {
            category: '=',
            filter: '&onFilter',
            filterAttr: '@onFilter',
			active: '='
        }
    };
})
.directive('expenseGrid', function () {
	return {
		templateUrl: 'expense-grid.html',
		scope: {
			expenses: '=',
			filter: '='
		}
	};
})
.run(['$rootScope', '$location', function ($rootScope, $location) {
	$rootScope.expenseCategories = { "1": "Household", "2": "Gas", "3": "Entertainment", "4": "Durable", "5": "Vacation", "6": "TutorTime", "7": "Toll", "8": "Periodical" };
	$rootScope.getMonthSum = function () {
		return $location.path().substr(0, 9) === '/monthly/' ? $rootScope.monthSum : null;
	}
}])
;
