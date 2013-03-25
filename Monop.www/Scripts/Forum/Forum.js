$(document).ready(function () {
   
    $(window).scroll(function () {
        if (($(window).scrollTop() + 1) == ($(document).height() - $(window).height())) {
            
            var lastId = $('#productsDiv div.product:last').attr('id');
            var tid = $('#htid').val();
            GetPosts(lastId, tid);
        };
    });
});

function GetPosts(lastId, tid) {
    $('div#lastPostsLoader').html('<img src="/Content/bigLoader.gif" />');
    $.post({'/Forum/PostsPart',
        data: 'lastId=' + lastId + '&tid=' + tid,
        dataType: "html",
        success: function (result) {
            var domElement = $(result);
            $('#productsDiv').append(domElement);
        }
    });
    $('div#lastPostsLoader').empty();
};