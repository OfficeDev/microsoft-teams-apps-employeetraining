// <copyright file="timepicker.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import React, { useState } from 'react';
import { Input, Popup, Flex, Dropdown, Text } from '@fluentui/react-northstar';
import { Icon } from '@fluentui/react/lib/Icon';
import { LanguageDirection } from '../../../models/language-direction';
import { useTranslation } from 'react-i18next';

import "./timepicker.css";

interface ITimePickerProps {
    onPickerClose: (hours: number, minutes: number) => void,
    hours?: number,
    minutes?: number,
    minHours?: number,
    minMinutes?: number,
    isDisabled: boolean,
    dir: LanguageDirection
}

export const TimePicker: React.FunctionComponent<ITimePickerProps> = props => {
    const localize = useTranslation().t;
    const [open, setOpen] = useState(false);
    const [text, setText] = React.useState(props.hours!.toString().padStart(2, '0') + ":" + props.minutes!.toString().padStart(2, '0'));
    const [hours, setHour] = React.useState(props.hours!.toString().padStart(2, '0'));
    const [minutes, setMinute] = React.useState(props.minutes!.toString().padStart(2, '0'));
    const [minHours, setMinHour] = React.useState(props.minHours!);
    const [minMinutes, setMinMinute] = React.useState(props.minMinutes!);

    const hoursItems: Array<string> = [];
    const minutesItems: Array<string> = [];

    React.useEffect(() => {
        setHour(props.hours!.toString().padStart(2, '0'));
        setText(props.hours!.toString().padStart(2, '0') + ":" + props.minutes!.toString().padStart(2, '0'));
    }, [props.hours]);
    React.useEffect(() => {
        setMinute (props.minutes!.toString().padStart(2, '0'));
        setText(props.hours!.toString().padStart(2, '0') + ":" + props.minutes!.toString().padStart(2, '0'));
    }, [props.minutes]);
    React.useEffect(() => {
        setMinHour(props.minHours!);
    }, [props.minHours]);
    React.useEffect(() => {
        setMinMinute(props.minMinutes!);
    }, [props.minMinutes]);

    for (var i = minHours ? minHours : 0 ; i < 24; i++) {
        hoursItems.push(i.toString().padStart(2, '0'));
    }
    for (var i = 0; i < 60; i++) {
        minutesItems.push(i.toString().padStart(2, '0'));
    }

    const onHourChange = {
        onAdd: item => {
            if (item) {
                setHour(item);
            }
            return "";
        }
    }

    const onMinuteChange = {
        onAdd: item => {
            if (item) {
                setMinute(item);
            }
            return "";
        }
    }

    const onOpenChange = (e, { open }: any) => {
        setOpen(open)
        if (!open) {
            setText(hours + ":" + minutes);
            props.onPickerClose(parseInt(hours), parseInt(minutes));
        }
    }

    const popupContent = (
        <div className="timepicker-popup-style">
            <Flex gap="gap.small">
                <Flex.Item size="size.half">
                    <Text size={props.dir === LanguageDirection.Rtl ? "medium" : "small"} content={localize("hourPlaceholder")} />
                </Flex.Item>
                <Flex.Item size="size.half" className={props.dir === LanguageDirection.Rtl ? "rtl-right-margin-large" : ""}>
                    <Text size={props.dir === LanguageDirection.Rtl ? "medium" : "small"} content={localize("minutePlaceholder")} />
                </Flex.Item>
            </Flex>
            <Flex gap="gap.small" styles={{marginTop:"0.5rem"}}>
                <Flex.Item>
                    <Dropdown
                        className="timepicker-dropdown"
                        items={hoursItems}
                        value={hours}
                        placeholder={localize("hourPlaceholder")}
                        getA11ySelectionMessage={onHourChange}
                    />
                </Flex.Item>
                <Flex.Item>
                    <Dropdown
                        className={props.dir === LanguageDirection.Rtl ? "timepicker-dropdown rtl-right-margin-small" : "timepicker-dropdown"}
                        value={minutes}
                        items={minutesItems}
                        placeholder={localize("minutePlaceholder")}
                        getA11ySelectionMessage={onMinuteChange}
                    />
                </Flex.Item>
            </Flex>
        </div>
    )
    return (
        <Popup
            open={open}
            trapFocus
            onOpenChange={onOpenChange}
            trigger={<Input
                className={props.dir === LanguageDirection.Rtl ? "rtl-left-margin-small" : ""}
                disabled={props.isDisabled} fluid value={text} icon={<Icon iconName="Clock" />}
                iconPosition={props.dir === LanguageDirection.Rtl ? "start" : "end"}
            />}
            content={popupContent}
        />
    )
}

TimePicker.defaultProps = {
    hours: 0,
    minutes: 0
};