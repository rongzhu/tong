'use strict';

angular.module('search-view', ['ngRoute'])
    .config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/search', {
            templateUrl: 'views/search-view.html',
            controller: 'searchController',
            controllerAs: 'search'
        });
    }])
    .controller('searchController', ['$http', function ($http) {
        var vm = this;

        vm.doSearch = function () {
            $http.post('/expensedata/search', vm)
            .then(function (resp) {
                vm.data = resp.data;
            });
        }
    }])
;
