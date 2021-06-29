// <copyright file="toast-notification.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { IToastNotification } from "../../models/IToastNotification";
import { ActivityStatus } from "../../models/activity-status";
import { Toast } from "react-bootstrap";
import { LanguageDirection } from "../../models/language-direction";

import "./toast-notification.css";

interface IToastNotificationProps extends WithTranslation {
    notification: IToastNotification,
    dir: LanguageDirection
}

interface IToastNotificationState {
    isShowNotification: boolean
}

/**
 * The toast notification which shows the recent status messages
 * @param props The props of type IToastNotificationProps
 */
class ToastNotification extends React.Component<IToastNotificationProps, IToastNotificationState> {
    readonly localize: TFunction;

    constructor(props) {
        super(props);

        this.localize = this.props.t;

        this.state = {
            isShowNotification: false
        }
    }

    componentWillReceiveProps(nextProps: IToastNotificationProps) {
        if (nextProps.notification.id !== this.props.notification.id && nextProps.notification.message?.length && nextProps.notification.type !== ActivityStatus.None) {
            this.setState({ isShowNotification: true });
        }
    }

    onClose = () => {
        this.setState({ isShowNotification: false });
    }

    render() {
        return (
            <Toast
                className={`notification-toast ${this.props.notification.type === ActivityStatus.Success ? "success" : "error"}`}
                autohide
                delay={4000}
                show={this.state.isShowNotification}
                onClose={this.onClose}>
                <Toast.Body className={this.props.dir === LanguageDirection.Ltr ? "notification" : "rtl-notification"}>
                    <strong>{this.props.notification.message}</strong>
                </Toast.Body>
            </Toast>
        );
    }
}

export default withTranslation()(ToastNotification);