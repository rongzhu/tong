//var s2 = document.createElement('script'); s2.src = 'https://cdnjs.cloudflare.com/ajax/libs/jquery/2.1.4/jquery.js'; document.head.appendChild(s2);
//setTimeout(workIt, 1000);

(function () {
	function pad0(m) {
		return m < 10 ? '0' + m.toString() : m.toString();
	}

	var url, host = 'http://www.tongbrothers.com';	//'localhost:51295';//
	var t = new Date();
	var e = new Date(t.getFullYear(), t.getMonth(), t.getDate() - 1);
	var s = new Date(e.getFullYear(), e.getMonth() - 2, e.getDate());
	var tempPostUrl = 'https://pages.mushroomnetworks.com/radio/show_csv.aspx?func=201';

	if (window.location.origin.indexOf('capital') >= 0) {
		//captial
		url = window.location.origin + '/api/accounts/d3756f74b6e2bcddcd86708531dc029de713e4f756fd5685889d60b9652291d7/transactions/file?fileType=csv&includePending=false';
		url += '&startDate=' + s.getFullYear() + '-' + pad0(s.getMonth() + 1) + '-' + pad0(s.getDate());
		url += '&endDate=' + e.getFullYear() + '-' + pad0(e.getMonth() + 1) + '-' + pad0(e.getDate());
		$.get(url, null, function (data) {
			$.post(tempPostUrl, { data: data }, function () {
				window.open(host + '/ng/entry?rtrv=Capital One');
			});
		});
	} else if (window.location.origin.indexOf('chase') >= 0 || true && window.location.hash.indexOf('summary/creditCard') >= 0) {
		//chase cc
		url = "https://secure07b.chase.com/svc/rr/accounts/secure/v1/account/activity/download/count/dda/list";
		var postBody = {
			accountId: '410010245',
			dateHi: s.getFullYear() + pad0(s.getMonth() + 1) + pad0(s.getDate()),
			dateLo: e.getFullYear() + pad0(e.getMonth() + 1) + pad0(e.getDate()),
			downloadType: 'CSV',
			transactionType: 'ALL'
		};
		$.post(url, postBody, function (data) {
				console.log(data);
		
			if (data && data.Posted && data.Posted.length > 0) {
				var csv = data.Posted.map(function(x) {
					return 'RONGCHCC,' + x.tranDate + ',' + (x.merchantName || '').replace(/,/g, '') + ',' + x.amount;
				}).join('\n');

				$.post(tempPostUrl, { data: csv }, function () {
					window.open(host + '/ng/entry?rtrv=Chase Credit').focus();
					window.location.href = 'https://banking.chase.com/AccountActivity/AccountDetails.aspx?AI=410010245';
				});
			} else {
				console.log(data);
			}
		});
	} else if (window.location.origin.indexOf('chase') >= 0 && window.location.hash.indexOf('summary/dda') >= 0) {
		//chase chk
		var csv = '';
		var rows = $('#DDATransactionDetails_DDAPrepaidTrnxDetailsGrid tr.divider3').each(function () {
			var date = $(this).find('td:nth-child(1) span').text();
			if (date.indexOf('Pending') === -1) {
				csv += 'RONGCHCK,' + date + ',';	//date
				csv += $(this).find('td:nth-child(2)').text() + ',';		//type
				csv += $(this).find('td:nth-child(3)').text().replace(/,/g, '') + ',';		//desc
				csv += $(this).find('td:nth-child(4)').text().replace(/,/g, '') + ',';		//debit
				csv += $(this).find('td:nth-child(5)').text().replace(/,/g, '') + '\n';		//credit
			}
		});

		$.post(tempPostUrl, { data: csv }, function () {
			window.open(host + '/ng/entry?rtrv=Chase Checking').focus();
			window.location.href = 'https://cards.chase.com/cc/Account/Activity/454724560';
		});
	}

})();

//javascript: var s1 = document.createElement('script'); s1.src = 'https://pages.mushroomnetworks.com/radio/test1.js'; document.head.appendChild(s1);
//javascript: var s1 = document.createElement('script'); s1.src = 'https://e86ddaa4.ngrok.io/test1.js'; document.head.appendChild(s1);
