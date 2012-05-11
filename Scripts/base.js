$(function () {
    $("#like").click(function () {
        var boo = $(this).hasClass("disabled");
        if (boo) return false;

        var c_id = $("#competition_id").val();
        var p_id = $("#participant_id").val();

        $.post("/Competition/Like/" + c_id, { participant_id: p_id }, function (data) {
            $("#likes").html(data);
        });

        $(this).addClass("disabled");
    });

    $(".link").click(function(){
        var href = $(this).attr("href");
        location.href = href;
        return false;
    });

    $("#add_comment").click(function () {
        var c_id = $("#competition_id").val();
        var p_id = $("#participant_id").val();
        var text = $("#comment_text").val();
        console.log(c_id + " " + p_id + " " + text);
        $.post("/Competition/Comment/" + c_id, { participant_id: p_id, text: text }, function (data) {
            window.location.reload(true);
        });
        return false;
    });

});