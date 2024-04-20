function renderCrazorCard(cardId, channelId, botUri, routeUrl, card) {
    function getCookie(cookieName) {
        var name = cookieName + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i].trim();
            if ((c.indexOf(name)) == 0) {
                return c.substr(name.length);
            }

        }
        return null;
    }
    const name = new URLSearchParams(window.location.search).get("name");
    let cardDiv = document.getElementById(cardId);

    let adaptiveCard = new AdaptiveCards.AdaptiveCard();
    adaptiveCard.hostConfig = adaptiveHostConfig;

    window.history.replaceState({}, null, routeUrl);

    const invokeBot = (invokeName, invokeValue) =>
        new Promise((resolve, reject) => {

            $(`#${cardId}`).hover(function () {
                $(this).css("cursor", "progress");
            }, function () {
                $(this).css("cursor", "default");
            });
            $.ajax({
                type: 'POST',
                url: botUri,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: JSON.stringify({
                    serviceUrl: 'http://blank',
                    id: new Date().getTime().toString(),
                    type: 'invoke',
                    channelId: channelId,
                    conversation: { id: getCookie('sharedId') },
                    from: { id: getCookie('userId'), name: name },
                    to: { id: 'bot id' },
                    timestamp: new Date().toISOString(),
                    localTimestamp: new Date().toLocaleString("en-US", { timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone }),
                    localTimezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
                    name: invokeName,
                    value: invokeValue
                }, null, 2),
                beforeSend: function (xhr) {
                    var token = getCookie("token");
                    if (token.length > 0)
                        xhr.setRequestHeader('Authorization', 'Bearer ' + token);
                },
                error: function (invokeResponse, status, xhr) {
                    $(`#${cardId}`).unbind('mouseenter mouseleave');
                    reject(status);
                },
                success: function (invokeResponse, status, xhr) {   // success callback function
                    adaptiveCard.parse(invokeResponse.value);
                    if (invokeResponse.value.metadata.webUrl != null && window.location.pathname != invokeResponse.value.metadata.webUrl) {
                        window.history.replaceState({}, null, invokeResponse.value.metadata.webUrl);
                        document.title = invokeResponse.value.title;
                    }
                    cardDiv.replaceChild(adaptiveCard.render(), cardDiv.children[0]);
                    $(`#${cardId}`).unbind('mouseenter mouseleave');
                    resolve(status);
                }
            });
        });

    const tryGetToken = () => {
        const input = document.getElementById('token');
        const token = input.value.trim();
        return token;
    }

    adaptiveCard.onExecuteAction = function (actionIn) {
        var action = actionIn.toJSON();
        action.data = actionIn.data;
        switch (action.type) {
            case 'Action.OpenUrl':
                window.open(action.url, "_blank");
                break;
            case 'Action.Execute':
                invokeBot('adaptiveCard/action', { action: action });
                break;
        }

    };
    adaptiveCard.parse(card);
    cardDiv.appendChild(adaptiveCard.render());
}