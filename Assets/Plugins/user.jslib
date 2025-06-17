mergeInto(LibraryManager.library, {

    Hello: function () {
        console.log("Hello");
    },

    getUserNickname: function () 
    {
        console.log("Get Nickname Inicialise");

        console.log(window.x1_unity_data.user.nickname);

        var returnStr = window.x1_unity_data.user.nickname;
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    },

});




