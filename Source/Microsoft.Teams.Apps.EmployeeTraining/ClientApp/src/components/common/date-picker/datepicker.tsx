// <copyright file="date-picker.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import moment from "moment";
import 'moment/min/locales.min';
import { Flex } from '@fluentui/react-northstar';
import { useState } from "react";
import { DatePicker } from 'office-ui-fabric-react/lib/DatePicker';
import { Fabric, Customizer, IDatePickerStrings } from 'office-ui-fabric-react';
import { initializeIcons } from 'office-ui-fabric-react/lib/Icons';
import { DarkCustomizations } from "../../../helpers/dark-customizations";
import { DefaultCustomizations } from "../../../helpers/default-customizations";
import Constants from "../../../constants/resources";
import "./date-picker.css";

initializeIcons();

interface IDateePickerProps {
    selectedDate: Date;
    onDateSelect: (startDate: Date) => void,
    theme: string,
    screenWidth: number,
    minDate: Date;
    disableSelectionForPastDate:boolean
}

let dayPickerStrings: IDatePickerStrings = {
    months: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"],
    shortMonths: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
    days: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"],
    shortDays: ["S", "M", "T", "W", "T", "F", "S"]
};

const onFormatDate = (date?: Date): string => {
    return !date ? '' : moment.utc(date).local().format("LL");
};

const StartDate: React.FC<IDateePickerProps> = props => {
    let bgcolor = "";
    let theme = props.theme;
    let datePickerTheme;

    dayPickerStrings.months = moment.months();
    dayPickerStrings.shortMonths = moment.monthsShort();
    dayPickerStrings.days = moment.weekdays();
    dayPickerStrings.shortDays = moment.weekdaysShort();

    if (theme === Constants.dark) {
        bgcolor = "dark-datepicker";
        datePickerTheme = DarkCustomizations
    }
    else if (theme === Constants.contrast) {
        bgcolor = "dark-datepicker";
        datePickerTheme = DarkCustomizations
    }
    else {
        bgcolor = "default-datepicker";
        datePickerTheme = DefaultCustomizations
    }

    const [selectedDate, setDate] = useState<Date | null | undefined>(props.selectedDate);
    const [minDate, setMinDate] = useState<Date | null | undefined>(props.minDate);

    React.useEffect(() => {
        setDate(props.selectedDate);
    }, [props.selectedDate]);

    React.useEffect(() => {
        setMinDate(props.minDate);
    }, [props.minDate]);

    /**
       * Handle change event for cycle start date picker.
       * @param date | cycle start date.
       */
    const onSelectStartDate = (date: Date | null | undefined): void => {
        let startCycle = moment(date)
            .set('hour', moment().hour())
            .set('minute', moment().minute())
            .set('second', moment().second());
        props.onDateSelect(startCycle.toDate()!);
        setDate(startCycle.toDate());
    };

    return (
        <>
            {
                <Flex gap="gap.small">
                    <Fabric className="full-width">
                        <Customizer {...datePickerTheme}>
                            <DatePicker
                                className={bgcolor}
                                label={''}
                                strings={dayPickerStrings}
                                formatDate={onFormatDate}
                                showMonthPickerAsOverlay={true}
                                minDate={minDate!}
                                isMonthPickerVisible={true}
                                value={selectedDate!}
                                onSelectDate={onSelectStartDate}
                                disabled={props.disableSelectionForPastDate}
                            />
                        </Customizer>
                    </Fabric>
                </Flex>
            }
        </>
    );
}

export default StartDate;
