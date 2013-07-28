function roll(n) {
    $.post("/Ajax/Roll",
            { roll: n },
            function (data) {
                UpdatePage(data);
               
            });
}

function action(act) {

    $.post("/Ajax/Go",
        { act: act },
        function (data) {
            UpdatePage(data);
           
        });
}

function next() {
    $.post("/Game/Next", null,
        function (data) {
            UpdatePage(data);
        });
}

function loadPageData() {
    $.post("/Ajax/GameInfo", null,
        function (data) {
            UpdatePage(data);
        }
    );
}

function UpdatePage(data) {

    $.each(data.Map, function (i, item) {
        //alert(item.id + '===' + item.text);
        $('#map_c' + item.id).html(item.text).css('backgroundColor', item.color);

    });
    $.each(data.Players, function (i, item) {
        //alert(item.id + '===' + item.text);
        $('#map_c' + item.id).html(item.images);
    });
   
    $("#plstate").html(data.PlayersState);
    $("#log").html(data.GameLog);
}

function chSpeedUpdate() {
    var ch = $('#speedUpd').is(':checked');
    if (ch) {
        clearInterval(updater);
        updater = setInterval('loadPageData()', 2000);
    } else {
        clearInterval(updater);
        updater = setInterval('loadPageData()', 1000);
    }


}



function sendMessage() {
    $.post("/Ajax/SendGameMessage", { mes: $("#chatMessage").val() },
        function (data) {
            //$("#divChat").html(data);
        }
    );
    $("#chatMessage").val("");
}





