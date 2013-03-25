 $(document).ready(function () {

         

            var dialogOpts1 = {
                title: "Show trades",
                autoOpen: false,
                height: 500,
                width: 500,
            };

            $("#divTrades").dialog(dialogOpts1);    //end dialog

            $('#ButtonShowTrades').click(function () {
            
                 var dialog = $("#divTrades").dialog('open'); ;
                        // load remote content
                        dialog.load(
                                "/Ajax/ShowTrades", 
                                {},
                                function (responseText, textStatus, XMLHttpRequest) {
                                        dialog.dialog();
                                }
                        );
              
              
            });


             var dialogOpts = {
                title: "Player lands",
                autoOpen: false,
                height: 500,
                width: 500,
            };
            $("#divPlayerCells").dialog(dialogOpts);    //end dialog

            $('#btnRefresh').click(function () {
            
                 var dialog = $("#divPlayerCells").dialog('open');
                        // load remote content
                        dialog.load(
                                "/Ajax/PlayerCells", 
                                {},
                                function (responseText, textStatus, XMLHttpRequest) {
                                        dialog.dialog();
                                }
                        );
              
            });
        });