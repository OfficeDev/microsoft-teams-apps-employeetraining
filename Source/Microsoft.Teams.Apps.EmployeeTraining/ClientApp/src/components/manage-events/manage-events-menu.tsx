// <copyright file="manage-events-menu.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { IEvent } from "../../models/IEvent";
import { EventStatus } from "../../models/event-status";
import { MenuButton, Button, MenuItemProps, MenuShorthandKinds, ShorthandCollection, BellIcon } from "@fluentui/react-northstar";
import { UserFriendsIcon, EditIcon, ArrowDownIcon, CloseIcon, MoreIcon } from "@fluentui/react-northstar";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import moment from "moment";
import { LanguageDirection } from "../../models/language-direction";

interface IManageEventsMenuProps extends WithTranslation {
    eventDetails: IEvent,
    onCloseRegistration: (eventId: string) => void,
    onEditEvent: (eventId: string) => void,
    onExportDetails: (eventId: string, eventName: string) => void,
    onSendReminder: (eventId: string) => void,
    onCancelEvent: (eventId: string) => void,
    onDeleteDraftEvent: (eventId: string, eventName: string) => void,
    dir: LanguageDirection
}

/**
 * This component renders menu based on event status
 * @param props The props of type IManageEventsMenu
 */
const ManageEventsMenu: React.FunctionComponent<IManageEventsMenuProps> = props => {
    const localize: TFunction = props.t;

    /** Gets menu items for active events */
    const getActiveEventsMenu: ShorthandCollection<MenuItemProps, MenuShorthandKinds> = [
        {
            icon: <EditIcon outline />,
            content: localize("editEventInformation"),
            onClick: () => props.onEditEvent(props.eventDetails.eventId)
        },
        {
            icon: <ArrowDownIcon />,
            content: localize("exportRegistrationDetails"),
            onClick: () => props.onExportDetails(props.eventDetails.eventId, props.eventDetails.name)
        },
        {
            icon: <BellIcon outline />,
            content: localize("sendReminder"),
            onClick: () => props.onSendReminder(props.eventDetails.eventId)
        },
        {
            kind: "divider"
        },
        {
            icon: <CloseIcon outline />,
            content: localize("cancelEvent"),
            onClick: () => props.onCancelEvent(props.eventDetails.eventId)
        }
    ]

    /** Gets menu items for draft events */
    const getDraftEventsMenu: ShorthandCollection<MenuItemProps, MenuShorthandKinds> = [
        {
            icon: <EditIcon outline />,
            content: localize("editEventInformation"),
            onClick: () => props.onEditEvent(props.eventDetails.eventId)
        },
        {
            kind: "divider"
        },
        {
            icon: <CloseIcon outline />,
            content: localize("deleteDraft"),
            onClick: () => props.onDeleteDraftEvent(props.eventDetails.eventId, props.eventDetails.name)
        }
    ]

    /** Gets menu items for completed events */
    const getCompletedEventsMenu: ShorthandCollection<MenuItemProps, MenuShorthandKinds> = [
        {
            icon: <ArrowDownIcon />,
            content: localize("exportRegistrationDetails"),
            onClick: () => props.onExportDetails(props.eventDetails.eventId, props.eventDetails.name)
        }
    ]

    /** Gets menu based on event status */
    const getMenuItems = () => {
        switch (props.eventDetails.status) {
            case EventStatus.Draft:
                return getDraftEventsMenu;

            case EventStatus.Active:
                if (new Date() < moment.utc(props.eventDetails.endDate).local().toDate()) {
                    if (props.eventDetails.isRegistrationClosed) {
                        return getActiveEventsMenu;
                    }

                    getActiveEventsMenu.unshift(
                        {
                            icon: <UserFriendsIcon outline />,
                            content: localize("closeRegistration"),
                            onClick: () => props.onCloseRegistration(props.eventDetails.eventId)
                        });

                    return getActiveEventsMenu;
                }

                return getCompletedEventsMenu;

            default:
                break;
        }
    }

    return (
        <MenuButton
            trigger={<Button text iconOnly icon={<MoreIcon />} />}
            menu={{
                items: getMenuItems()
            }}
            position="before"
            className={ props.dir === LanguageDirection.Rtl ? "ltr-direction" : ""}
        />
    );
}

export default withTranslation()(ManageEventsMenu);