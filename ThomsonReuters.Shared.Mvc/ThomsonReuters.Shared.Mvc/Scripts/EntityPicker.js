function initEntityPickerAutoComplete(acEmlId, postUrl, oMinLen, oDelay, oAutoFocus, isDisabled) {
    var jacElm = $(acEmlId);
    var mainContainer = jacElm.parent();
    var isMulti = mainContainer.attr("isMulti");

    etyPickerInitContainer(mainContainer, isDisabled);

    $("form").bind('invalid-form.validate', etyPickerScrollToInvalidPicker);

    if (!isDisabled) {
        jacElm.autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: postUrl,
                    type: "POST",
                    dataType: "json",
                    data: { text: request.term, excludeValues: etyPickerGetExcludedValues(acEmlId) },
                    cache: false,
                    success: function (data) {
                        response($.map(data, function (item) {
                            return { label: item.Text, value: item.Value, id: item.Text };
                        }));
                    }
                });
            },
            select: function (e, ui) {
                if (ui.item.label == "No match found") {
                    return false;
                }
                if (isMulti) {
                    entityPickerSelectItemMulti(mainContainer, ui.item);
                }
                else {
                    entityPickerSelectItemSingle(mainContainer, ui.item);
                }
                return false;
            },
            close: function () {
                jacElm.val("");
            },
            position: {
                my: "left top",
                at: "left bottom",
                collision: "none"
            },
            minLength: oMinLen,
            delay: oDelay,
            autoFocus: oAutoFocus
        });
    }
}

function etyPickerInitContainer(mainContainer, isDisabled) {
    // Empty Text state
    var itemsContainer = mainContainer.children('.etyItemsContainer');
    var etyItems = itemsContainer.children(".etyItem");
    var acInput = mainContainer.children(".etyPckrInput");
    var isMulti = mainContainer.attr("isMulti") ? true : false;
    var isMarkingEnabled = etyPickerHasMarkingEnabled(mainContainer);

    etyPickerHandleRequiredFieldIndicator(mainContainer);

    if (!isDisabled) {
        mainContainer.click(etyPickerClick);
        acInput.focusin(etyPickerFocusIn);
        acInput.focusout(etyPickerFocusOut);
    }
    else {
        acInput.hide();
    }
    if (etyItems.length > 0) {
        itemsContainer.children(".etyPkrEmptyText").hide();
    }
    if (!isDisabled) {
        // Entity item events registering
        for (var i = 0; i < etyItems.length; i++) {
            var jEmp = $(etyItems[i]);
            registerEntityItemMouseMoveEvents(jEmp);
            if (isMulti) {
                registerEntityItemClickEventMulti(jEmp);
            }
            else {
                registerEntityItemClickEventSingle(jEmp);
            }
        }
    }
    if (isMarkingEnabled && !isDisabled) {
        etyPickerInitMarking(mainContainer);
    }
}

function etyPickerInitMarking(mainContainer) {
    var mainCId = "#" + mainContainer[0].id;
    var menuUl = $(mainCId + "MarkMenu");
    var markHidField = mainContainer.children(".etyPkrMarkHidFld");

    menuUl.hide().menu({
        select: function (event, ui) {
            var doSelect = ui.item.attr("marksel") ? true : false;
            var forEntity = menuUl.data("forEty");
            var forEntityId = forEntity.parent().attr("etyid");
            mainContainer.find(".etyItemMarkBtn").removeClass("selected");
            if (doSelect) {
                markHidField.val(forEntityId);
                forEntity.addClass("selected");
            }
            else {
                markHidField.val(null);
            }
        }
    });

    registerEntityItemMenuButtonClickEvents(mainContainer);
}

function etyPickerHandleRequiredFieldIndicator(mainContainer) {
    var hid = mainContainer.children(".etyPkrHidFld");
    if (hid.attr("data-val-reqfldind") != null) {
        mainContainer.addClass("req-fld-ind");
    }
}
//---------- Select From List ----------
function entityPickerSelectItemSingle(mainContainer, ui) {
    if (ui != null && ui.value != null && ui.value != "") {
        var etyItemContainer = mainContainer.children(".etyItemsContainer");
        var etyItems = etyItemContainer.children(".etyItem");
        var acInput = mainContainer.children(".etyPckrInput");
        var hid = mainContainer.children(".etyPkrHidFld")[0];

        // Set postback value
        hid.value = ui.value;

        // Remove existing item if any
        etyItems.remove();

        // Create and add new entity item and register events
        var newItemStr = String().concat("<span class='etyItem' etyID='", ui.value, "'>", ui.label, "</span>");
        etyItemContainer.append(newItemStr);

        var newItem = etyItemContainer.children(".etyItem:last");
        registerEntityItemMouseMoveEvents(newItem);
        registerEntityItemClickEventSingle(newItem);

        // Clear auto complete textbox
        acInput[0].value = "";

        // Show hide auto complete
        etyPickerShowHideACInput(mainContainer);

        // trigger a change on the hidden field for those subscribed to this event
        $(hid).trigger('change');
    }
}

function entityPickerSelectItemMulti(mainContainer, ui) {
    if (ui != null && ui.value != null && ui.value != "") {
        var etyItemContainer = mainContainer.children(".etyItemsContainer");
        var acInput = mainContainer.children(".etyPckrInput");
        var hid = mainContainer.children(".etyPkrHidFld")[0];
        var isWithinML = isWithinkMultiLimit(mainContainer);
        var isMarkingEnabled = etyPickerHasMarkingEnabled(mainContainer);

        if (!isEntityValueAlreadyPresent(hid.value, ui.value) && isWithinML) {
            // Set postback value
            addEntityIdValue(hid, ui.value);

            // Create and add new entity item and register events
            var markingButton = isMarkingEnabled ? "<span class='etyItemMarkBtn'></span>" : "";
            var newItemStr = String().concat("<span class='etyItem' etyID='", ui.value, "'>", markingButton, ui.label, "</span>");
            etyItemContainer.append(newItemStr);

            var newItem = etyItemContainer.children(".etyItem:last");
            registerEntityItemMouseMoveEvents(newItem);
            registerEntityItemClickEventMulti(newItem);
            registerEntityItemMenuButtonClickEvents(mainContainer);

            // trigger a change on the hidden field for those subscribed to this event
            $(hid).trigger('change');
        }

        // Clear auto complete textbox
        acInput[0].value = "";

        // Show hide auto complete
        etyPickerShowHideACInput(mainContainer);
    }
}

//---------- Showing and Hiding ----------

function etyPickerShowHideACInput(mainContainer, forceHide) {
    var acInput = mainContainer.children(".etyPckrInput");
    var etyItemsContainer = mainContainer.children(".etyItemsContainer");
    var emptyText = etyItemsContainer.children(".etyPkrEmptyText");
    var hasAnyEmps = etyItemsContainer.children(".etyItem").length > 0;
    var canShow = etyPickerCanShowACInput(mainContainer);
    if (!forceHide && canShow) {
        emptyText.hide();
        acInput.val(null);
        acInput.removeClass("etyPckrInputHide");
        acInput.prop("disabled", false);
        etyPickerHideValidationMessage(mainContainer);
        if (!acInput.is(":focus")) {
            acInput.focus();
        }
    }
    else {
        acInput.val(null);
        acInput.addClass("etyPckrInputHide");
        acInput.prop("disabled", true);

        acInput.autocomplete("close");
        if (hasAnyEmps) {
            emptyText.hide();
        }
        else {
            emptyText.show();
        }
    }
}
function etyPickerHideValidationMessage(mainContainer) {
    var valdMsg = mainContainer.children(".etyPkrValdMsg");
    var hidField = mainContainer.children(".etyPkrHidFld");

    valdMsg.removeClass("field-validation-error");
    hidField.removeClass("input-validation-error");

    valdMsg.addClass("field-validation-valid");
    valdMsg.text("");
}

//---------- Container Events ----------

function etyPickerFocusIn() {
    var mc = $(this).parent();
    etyPickerShowHideACInput(mc);
}

function etyPickerFocusOut(e) {
    var toelm = $(e.originalEvent.toElement).closest("ul.ui-autocomplete");

    if (toelm.length == 0) {
        etyPickerShowHideACInput($(this).parent(), true);
    }
}

function etyPickerClick() {
    var mc = $(this);
    etyPickerShowHideACInput(mc);
}

function etyPickerMarkButtonClick(e) {
    var me = $(this);
    var mainContainer = me.closest(".etyPkrContainer");
    var menuUl = mainContainer.children(".etyPkrMarkMenu");
    var markHidField = mainContainer.children(".etyPkrMarkHidFld");

    var forEntity = $(this);
    var forEntityId = forEntity.parent().attr("etyid");

    if (markHidField.val() == forEntityId) {
        menuUl.children().first().hide();
        menuUl.children().last().show();
    }
    else {
        menuUl.children().first().show();
        menuUl.children().last().hide();
    }

    menuUl.data("forEty", forEntity);
    menuUl.show()
        .position({
            my: "left top",
            at: "left bottom",
            of: $(this).parent()
        });

    $(document).one("click", function () {
        menuUl.hide();
    });

    e.preventDefault();
    return false;
}

//---------- Item Events ----------

function registerEntityItemMouseMoveEvents(newEtyItem) {
    newEtyItem.mouseenter(function () {
        $(this).addClass("etyItemDel");
        $(this)[0].title = "Click to remove this item.";
    });

    newEtyItem.mouseleave(function () {
        $(this).removeClass("etyItemDel");
    });
}

function registerEntityItemClickEventSingle(newetyItem) {
    newetyItem.click(function () {
        var me = $(this);
        var mainContainer = me.parent().parent();
        var hidFld = mainContainer.children(".etyPkrHidFld")[0];
        hidFld.value = "";

        me.remove();
        etyPickerShowHideACInput(mainContainer);

        // trigger a change on the hidden field for those subscribed to this event
        $(hidFld).trigger('change');

    });
}

function registerEntityItemClickEventMulti(newetyItem) {
    newetyItem.click(function () {
        var me = $(this);
        var mainContainer = me.parent().parent();
        var etyID = me[0].getAttribute("etyID");
        var hidFld = mainContainer.children(".etyPkrHidFld")[0];

        removeEntityIdValue(hidFld, etyID);

        me.remove();
        etyPickerShowHideACInput(mainContainer);

        // trigger a change on the hidden field for those subscribed to this event
        $(hidFld).trigger('change');
    });
}

function registerEntityItemMenuButtonClickEvents(mainContainer) {
    var mainCId = "#" + mainContainer[0].id;
    var mnuTriggerButtons = $(mainCId + " .etyItemMarkBtn");
    var bTitle = mainContainer.attr("markbtntooltip");

    if (bTitle) {
        mnuTriggerButtons.attr("title", bTitle);
    }

    mnuTriggerButtons.unbind("click", etyPickerMarkButtonClick);
    mnuTriggerButtons.bind("click", etyPickerMarkButtonClick);
}

//--------- Utilities ---------

function etyPickerClear(id) {
    var el = $("#" + id);
    var items = el.parent().find(".etyItem");

    el.val(null);
    items.remove(".etyItem");
    el.parent().find(".etyPkrEmptyText").show();
}

function etyPickerCanShowACInput(mainContainer) {
    var isMulti = mainContainer.attr("isMulti");
    if (isMulti) {
        return isWithinkMultiLimit(mainContainer);
    }
    else {
        return mainContainer.find(".etyItem").length == 0;
    }
}

function etyPickerGetExcludedValues(inputID) {
    var container = $(inputID).parent();
    var excludeValues = container.attr("excludeValues");
    return excludeValues;
}

function etyPickerScrollToInvalidPicker(form, validator) {
    if (validator.errorList != null && validator.errorList.length > 0) {
        var firstElm = $(validator.errorList[0].element);
        if (firstElm.hasClass("etyPkrHidFld")) {
            var container = firstElm.parent();
            $('html, body').animate({ scrollTop: container.offset().top }, 0);
        }
    }
}

function isEntityValueAlreadyPresent(currVals, newVal) {
    if (currVals != null && currVals != "") {
        var splits = currVals.split(",");
        for (var i = 0; i < splits.length; i++) {
            if (splits[i] == newVal) {
                return true;
            }
        }
    }
    return false;
}

function addEntityIdValue(hid, newVal) {
    var val = hid.value;
    if (val != null && val != "") {
        val = val + "," + newVal;
        hid.value = val;
    }
    else {
        hid.value = newVal;
    }
}

function removeEntityIdValue(hid, remVal) {
    var val = hid.value;
    if (val != null && val != "") {
        if (val.indexOf(remVal + ",") > -1) {
            val = val.replace(remVal + ",", "");
        }
        else if (val.indexOf("," + remVal) > -1) {
            val = val.replace("," + remVal, "");
        }
        else {
            val = val.replace(remVal, "");
        }
        hid.value = val;
    }
}

function isWithinkMultiLimit(mainContainer) {
    var ret = true;
    var multiLimit = mainContainer.attr("multiLimit");
    if (multiLimit != null && multiLimit != undefined && multiLimit != "") {
        var ml = parseInt(multiLimit.toString());
        ret = mainContainer.find(".etyItem").length < ml;
    }
    return ret;
}

function etyPickerHasAnyValue(id) {
    var ret = false;
    var el = $("#" + id);

    if (el.length > 0) {
        var values = el.val();
        if (values != null && values != undefined && values != "") {
            var items = values.split(",");
            ret = items.length > 0;
        }
    }

    return ret;
}

function etyPickerHasMarkingEnabled(mainContainer) {
    var isEnabled = !mainContainer.hasClass("etyDisabled");
    var hasMarkingAttr = (mainContainer.attr("isMarkingEnabled") == "true");
    return isEnabled && hasMarkingAttr;
}
