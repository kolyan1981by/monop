function Mortgage() {

    $.post("/Ajax/Mortgage",
        { str: state() },
        function(data) {
            //$("#map").html(data.Map);
            $("#log").html(data.GameLog);
            $("#divPlayerCells").html(data.PlayerCells);
        });

}

function Build(act) {

    $.post("/Ajax/Build",
        { str: state(), action: act },
        function(data) {
            //$("#map").html(data.Map);
            $("#log").html(data.GameLog);
            $("#divPlayerCells").html(data.PlayerCells);
        });

    }

function Sell() {

    $.post("/Ajax/SellHouses",
        { str: state() },
        function(data) {
            //$("#map").html(data.Map);
            $("#log").html(data.GameLog);
            $("#divPlayerCells").html(data.PlayerCells);
        });

    }

function state() {
    var str = "p0";
    $("input[name='0_ids[]']:checked").each(function(i) {
        str += "-" + $(this).val();
    });


    str += ";p1";
    $("input[name='1_ids[]']:checked").each(function(i) {
        str += "-" + $(this).val();
    });


    str += ";p2";
    $("input[name='2_ids[]']:checked").each(function(i) {
        str += "-" + $(this).val();
    });
    
    str += ";p3";
    $("input[name='3_ids[]']:checked").each(function(i) {
        str += "-" + $(this).val();
    });
    return str;
}

function Trade() {

    $.post("/Ajax/Trade",
        { str: stateForTrade() },
        function(data) {
            //$("#map").html(data.Mapinfo.Map);
            $("#log").html(data.GameLog);
            $("#divPlayerCells").html(data.PlayerCells);
        });

}

function stateForTrade() {

    var str = "p0";

    $("input[name='0_ids[]']:checked").each(function(i) {
        str += "-" + $(this).val();
    });

    str += "-m" + $("#0_m").val();

    str += ";p1";

    $("input[name='1_ids[]']:checked").each(function(i) {
        str += "-" + $(this).val();
    });
    str += "-m" + $("#1_m").val();

    str += ";p2";

    $("input[name='2_ids[]']:checked").each(function(i) {
        str += "-" + $(this).val();
    });
    str += "-m" + $("#2_m").val();

    str += ";p3";

    $("input[name='3_ids[]']:checked").each(function(i) {
        str += "-" + $(this).val();
    });
    str += "-m" + $("#3_m").val();

    return str;
}
function PlayerCells() {
    $.post("/Ajax/PlayerCells", null,
            function(data) { $("#PlayerCells").html(data); });
}