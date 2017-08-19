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
