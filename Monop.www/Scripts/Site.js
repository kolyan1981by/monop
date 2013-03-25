$(document).ready(function ($) {
    // Load dialog on page load

    $("#translate_modal").dialog({ autoOpen: false, height: 400, width: 600 });

    $('.trans_element').dblclick(function (e) {

        var id = $(this).attr('id');
        $('#elemId').val(id);
        $('#txtTranslate').text($(this).text());

        var dialog = $("#translate_modal").dialog('open'); ;

        $.getJSON("/Ajax/GetText/" + id, null,
                function (data) {
                    //alert("Data Loaded: " + data.ru+'='+data.en);
                    $("#TextEN").val(data.en);
                    $("#TextRU").val(data.ru);
                });

        return false;
    });

    $('#yes').click(function () {
        $.post("/Ajax/TranslateText",
                    { id: $('#elemId').val(), textEN: $("#TextEN").val(), textRU: $("#TextRU").val() },
                    function (response) {
                        $("#translate_modal").dialog('close');
                    });
    });

});