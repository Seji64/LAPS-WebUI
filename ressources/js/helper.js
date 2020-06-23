function LaraSelectedEventProxy(keyvalue, evt) { var $m_data = JSON.stringify(evt.params); LaraUI.sendMessage({ key: keyvalue, data: $m_data }); }

function setTooltip(btn, message) {
    $(btn).tooltip('hide')
        .attr('data-original-title', message)
        .tooltip('show');
}

function hideTooltip(btn) {
    setTimeout(function () {
        $(btn).tooltip('hide');
    }, 1000);
}