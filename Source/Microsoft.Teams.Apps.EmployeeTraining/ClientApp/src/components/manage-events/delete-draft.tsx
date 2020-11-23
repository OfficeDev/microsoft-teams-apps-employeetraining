// <copyright file="delete-draft.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { Flex, Text, Button, Provider } from "@fluentui/react-northstar";
import { useTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { deleteDraftEventAsync } from "../../api/manage-events-api";
import { ResponseStatus } from "../../constants/constants";

/** Renders component to delete draft event */
const DeleteDraftEvent: React.FunctionComponent = props => {
    const localize: TFunction = useTranslation().t;
    const search = window.location.search;
    const params = new URLSearchParams(search);
    const eventId = params.get("eventId")!;
    const teamId = params.get("teamId")!;
    let [isUpdatingStatus, setUpdatingEventStatus] = React.useState(false);
    let [errorGettingEventDetails, setEventDetailsError] = React.useState(false);
    let [errorUpdatingEventStatus, setUpdateStatusError] = React.useState(false);

    /** Delete draft event */
    const deleteDraftEvent = async () => {
        setUpdateStatusError(false);
        setUpdatingEventStatus(true);

        let response = await deleteDraftEventAsync(teamId!, eventId!);

        if (response && response.status === ResponseStatus.OK && response.data === true) {
            setUpdatingEventStatus(false);
            microsoftTeams.tasks.submitTask({ isSuccess: true });
        }
        else {
            setUpdatingEventStatus(false);
            setUpdateStatusError(true);
        }
    }

    /** Renders component */
    const renderEventDetails = () => {
        if (errorGettingEventDetails) {
            return <Text error content={localize("dataResponseFailedStatus")} weight="semibold" />
        }

        return (
            <Provider>
                <Flex>
                    <div className="task-module-container close-cancel-event-container">
                        <div className="close-cancel-event">
                            <Text weight="bold" content={localize("deleteConfirmationText")} />
                        </div>
                        <Flex styles={{ marginRight: "3.4rem", marginLeft: "3.4rem", margintop: "2rem", position:"absolute", bottom:"3.4rem" }} vAlign="center">
                            {errorUpdatingEventStatus ? <Text error content={localize("dataResponseFailedStatus")} weight="semibold" /> : null}
                            <Flex.Item push>
                                <Button
                                    primary
                                    loading={isUpdatingStatus}
                                    disabled={isUpdatingStatus}
                                    content={localize("delete")}
                                    onClick={deleteDraftEvent}
                                />
                            </Flex.Item>
                        </Flex>
                    </div>
                </Flex>
            </Provider>
        );
    }

    return renderEventDetails();
}

export default DeleteDraftEvent;