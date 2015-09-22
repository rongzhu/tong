'use strict';

angular.module('data-entry', ['ngRoute'])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/entry', {
            templateUrl: 'views/entry-view.html',
            controller: 'entryController',
            controllerAs: 'dataEntry'
        });
    }])
    .controller('entryController', ['$scope', '$timeout', '$http', '$location', function ($scope, $timeout, $http, $location) {
        var vm = this;

        $http.get('/expensedata/lastchargesdates')
        .success(function (data) { vm.lastChargesDates = data; });

        vm.onPaste = function () { $timeout(processExpensesRaw, 300); };

        var processExpensesRaw = function () {
            $http({
                url: '/expensedata/parseraw',
                method: 'POST',
                data: JSON.stringify(vm.expensesRaw),
                transformResponse: function (data) {
                    return JSON.parse(data, function (k, v) {
                        return k === 'TransactionDate' ? new Date(v) : v;
                    });
                }
            })
            .then(function (resp) {
                $scope.state.parsedExpenses = resp.data;
                $location.path('/entry/edit');
            });
        };
    }]);
