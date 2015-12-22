﻿'use strict';

angular.module('monthly-view', ['ngRoute'])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/monthly/:year/:month', {
            templateUrl: 'views/monthly-view.html',
            controller: 'monthlyController',
            controllerAs: 'monthly'
        });
    }])
    .controller('monthlyController', ['$routeParams', '$http', '$scope', '$rootScope', function ($routeParams, $http, $scope, $rootScope) {
        var vm = this;

        $scope.updateCurrentMonth($routeParams.year, $routeParams.month - 1);

        $http.get('/monthly/' + $routeParams.year + '/' + $routeParams.month)
        .then(function (resp) {
        	vm.data = resp.data;

        	$rootScope.monthSum = 0;
        	resp.data.Categories.forEach(x => $rootScope.monthSum += x.Amount);

            vm.expenseFilter = {};
        });
    }]);
