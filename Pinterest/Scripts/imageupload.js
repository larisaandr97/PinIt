
function uploadImage(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#imageprev')
                .attr('src', e.target.result);
        };

        reader.readAsDataURL(input.files[0]);
    }
}

function readURL() {
    $('#imageprev').attr('src', url.value);
}

function display() {
    $('#fromUrl').attr('style', "display: block;");
    console.log("1");
    return false;
}
