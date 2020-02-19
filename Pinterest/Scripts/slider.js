
/*$('.carousel[data-type="multi"] .item').each(function () {
    var next = $(this).next();
    if (!next.length) {
        next = $(this).siblings(':first');
    }
    next.children(':first-child').clone().appendTo($(this));

    for (var i = 0; i < 4; i++) {
        next = next.next();
        if (!next.length) {
            next = $(this).siblings(':first');
        }

        next.children(':first-child').clone().appendTo($(this));
    }
});*/




$(".slider_right").on("click", function () {
    var ul = $(this).parent().find("ul");
    var img_width = parseInt($(ul).find("img")[0].width) + 3;
    var ul_left = parseInt($(ul).css("left")) - img_width;
    var min_ul_left = -(ul.find("li").length - 4) * img_width;
    if (ul_left < min_ul_left) {
        ul_left = parseInt($(ul).css("left"));
    }
    ul.animate({
        "left": ul_left + "px",
    });
});

$(".slider_left").on("click", function () {
    var ul = $(this).parent().find("ul");
    var img_width = parseInt($(ul).find("img")[0].width) + 3;
    var ul_left = parseInt($(ul).css("left")) + img_width;
    if (ul_left > 0) {
        ul_left = 0;
    }
    ul.animate({
        "left": ul_left + "px",
    });
});

