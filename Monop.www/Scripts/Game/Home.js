

function showGames() {
    $.post("/Ajax/ShowGames", null,
        function(data) {
            $("#show_games").html(data.Games);
            if (data.NeedPlay == 'yes') window.location = "/Game/Play";
        }
    );
    
}

function showChat() {
    $.post("/Ajax/ShowChat", null,
        function (data) {
            $("#divChat").html(data);
        }
    );
   
}

function sendMessage() {
    $.post("/Ajax/SendMessage", { mes: $("#chatMessage").val() },
        function (data) {
            $("#divChat").html(data);
        }
    );
        $("#chatMessage").val("");
}

$(document).ready(
            function () {
                showGames();
                setInterval('showGames()', 4000);
                setInterval('showChat()', 2000);
            });



