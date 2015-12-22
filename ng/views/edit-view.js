'use strict';

angular.module('data-edit', ['ngRoute'])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/entry/edit', {
            templateUrl: 'views/edit-view.html',
            controller: 'editController',
            controllerAs: 'vm'
        });
    }])
    .controller('editController', ['$scope', '$http', '$location', function ($scope, $http, $location) {
        var vm = this;

        if (!$scope.state.parsedExpenses) {
            //$location.path('/entry');
            $scope.state.parsedExpenses = JSON.parse('[{"ExpenseID":0,"TransactionDate":"2015-08-13T00:00:00","Description":"THE CLASSIC Q          NEWPORT BEACH CA","Amount":14.96,"Category":"3","PaymentMethod":"Citi Credit"},{"ExpenseID":0,"TransactionDate":"2015-08-18T00:00:00","Description":"AMERICAN TABLE TENNIS  SOUTHFIELD    MI\\nDigital Account Number XXXXXXXXXXXX5924","Amount":124.49,"Category":"","PaymentMethod":"Citi Credit"},{"ExpenseID":0,"TransactionDate":"2015-08-19T00:00:00","Description":"BIGSHOTS BILLIARD BAR  LAKE FOREST   CA","Amount":7.43,"Category":"3","PaymentMethod":"Citi Credit"},{"ExpenseID":0,"TransactionDate":"2015-08-22T00:00:00","Description":"BIGSHOTS BILLIARD BAR  LAKE FOREST   CA","Amount":4.46,"Category":"3","PaymentMethod":"Citi Credit"},{"ExpenseID":0,"TransactionDate":"2015-08-26T00:00:00","Description":"BIGSHOTS BILLIARD BAR  LAKE FOREST   CA","Amount":4.05,"Category":"3","PaymentMethod":"Citi Credit"},{"ExpenseID":0,"TransactionDate":"2015-09-02T00:00:00","Description":"BIGSHOTS BILLIARD BAR  LAKE FOREST   CA","Amount":8.51,"Category":"3","PaymentMethod":"Citi Credit"}]',
                function (k, v) { return k === 'TransactionDate' ? moment(v).toDate() : v; });

            console.log($scope.state.parsedExpenses);
        }

        vm.save = function () {
            if ($scope.dataEntryForm.$valid) {
                $http.post('/expensedata/save', $scope.state.parsedExpenses)
                .then(function (resp) {
                    if (resp.data.badHints.length > 0 || resp.data.duplicateExpenses.length > 0) {
                        vm.data = resp.data;
                    }
                    else {
                        var now = new Date();
                        $location.path('/monthly/' + now.getFullYear() + '/' + (now.getMonth() + 1));
                    }
                });
            }
        };
    }]);
