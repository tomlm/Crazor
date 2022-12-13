export function renderCrazorCard(cardId, viewerRef, cardJson) {
    // const name = new URLSearchParams(window.location.search).get("name");
    let cardDiv = document.getElementById(cardId);

    let adaptiveCard = new AdaptiveCards.AdaptiveCard();
    adaptiveCard.hostConfig = adaptiveHostConfig;

    // window.history.replaceState({}, null, routeUrl);
    adaptiveCard.onExecuteAction = function (actionIn) {
        var action = actionIn.toJSON();
        action.data = actionIn.data;
        switch (action.type) {
            case 'Action.OpenUrl':
                window.open(action.url, "_blank");
                break;
            case 'Action.Execute':
                viewerRef.invokeMethodAsync('onExecuteAction', action );
                break;
            case 'Action.Submit':
                viewerRef.invokeMethodAsync('onExecuteAction', action);
                break;
        }

    };
    let card = JSON.parse(cardJson);
    adaptiveCard.parse(card);
    cardDiv.replaceChildren(adaptiveCard.render());
}