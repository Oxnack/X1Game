mergeInto(LibraryManager.library, {

    Hello: function () {
        console.log("Hello");
        window.alert("Hello, world!");
    },

    getUserNickname: function () 
    {
        console.log("Get Nickname Inicialise");
        window.alert("Get Nickname Inicialise");

        console.log(window.x1_unity_data.user.nickname);
        window.alert(window.x1_unity_data.user.nickname);
        return window.x1_unity_data.user.nickname;
    },

});




