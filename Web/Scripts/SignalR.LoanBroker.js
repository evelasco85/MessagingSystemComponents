﻿// Crockford's supplant method (poor man's templating)
if (!String.prototype.supplant) {
    String.prototype.supplant = function (o) {
        return this.replace(/{([^{}]*)}/g,
            function (a, b) {
                var r = o[b];
                return typeof r === 'string' || typeof r === 'number' ? r : a;
            }
        );
    };
}

$(function () {
    /*Signal-r Implementation*/
    var loanBroker = $.connection.loanBroker;

    function Initialize()
    {
    }

    $.connection.hub.start()
       .then(Initialize)
       .then(function () {
       })
       .done(function () {
       });

    /*************************/


    $('#btnAddLoan').on('click', function () {
        UpdateLoanRequest(3, 321, 123);
    });

    for (index = 1; index <= 30; index++) {
        LoadRandomizedLoanRequests(index);
    }

    function LoadRandomizedLoanRequests(ssn) {
        var min = 1;

        var loanAmount = ((Randomize(20) * 5000) + 25000);
        var loanTerm = (Randomize(72) + 12);

        AppendLoanRequest("", "", ssn, loanAmount, loanTerm);
    }

    function Randomize(max) {
        var min = 1;

        return Math.floor(Math.random() * (max - min + 1)) + min;
    }

    function AppendLoanRequest(clientId, requestId,
        ssn, loanAmount, loanTerm) {
        var newRow = $("<tr id='row_ssn_" + ssn + "'/>");

        newRow.append("<td>" + clientId + "</td>");
        newRow.append("<td>" + requestId + "</td>");
        newRow.append("<td>" + ssn + "</td>");
        newRow.append("<td>" + loanAmount + "</td>");
        newRow.append("<td>" + loanTerm + "</td>");
        newRow.append("<td id='interestRate'>" + 0 + "</td>");
        newRow.append("<td id='quoteId'>" + 0 + "</td>");

        var targetItemTable = $("#tblLoan > tbody");

        targetItemTable.append(newRow);
    }

    function UpdateLoanRequest(ssn, interestRate, quoteId) {
        var selector = "#row_ssn_" + ssn;
        var interestRateCell = $(selector + " > #interestRate");
        var quoteIdCell = $(selector + " > #quoteId");

        interestRateCell.text(interestRate);
        quoteIdCell.text(quoteId);
    }
});
