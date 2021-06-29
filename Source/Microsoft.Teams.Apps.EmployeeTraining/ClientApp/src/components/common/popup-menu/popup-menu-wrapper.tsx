// <copyright file="popup-menu-wrapper.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Popup, Button, Text } from "@fluentui/react-northstar";
import { ChevronDownIcon } from "@fluentui/react-icons-northstar";
import PopupMenuCheckboxesContent from "./popup-menu-checkboxes-content";
import PopupMenuRadiogroupContent from "./popup-menu-radiogroup-content";
import { ICheckBoxItem } from "../../../models/ICheckBoxItem";
import { LanguageDirection } from "../../../models/language-direction";

import "./popup-menu.css";

interface IPopupMenuWrapperProps {
    checkboxes?: Array<any>,
    radioGroup?: Array<any>,
    title: string,
    selectedSortBy?: number,
    showSearchBar?: boolean,
    selectedCount: number,
    onCheckboxStateChange: (typeState: Array<any>) => void,
    onRadiogroupStateChange: (selectedValue: number) => void,
    onOpenChange: (isOpen: boolean) => void,
    dir: LanguageDirection
}

const PopupMenuWrapper: React.FunctionComponent<IPopupMenuWrapperProps> = props => {
    const [popup, onOpenChange] = React.useState({ isOpen: false });
    let [disableClear, setdisableClear] = React.useState(true);

    const onFilterClick = () => {
        let checkCount = 0;
        let checkBox = props.checkboxes!;
        checkBox.map((checkbox: ICheckBoxItem) => {
            if (checkbox.isChecked) {
                checkCount = checkCount + 1;
            }
        });
        if (checkCount > 0) {
            setdisableClear(false);
        }
        else {
            setdisableClear(true);
        }
    }

    const onPopupOpenChange = (isOpen: boolean) => {
        onOpenChange({ isOpen: isOpen });
        props.onOpenChange(isOpen);
    }

    if (props.checkboxes) {
        return (
            <Popup
                className="popup-container"
                open={popup.isOpen}
                align="end"
                position="below"
                onOpenChange={(e, { open }: any) => onPopupOpenChange(open)}
                trigger={<Button data-testid={props.checkboxes.length > 0 ? props.checkboxes[0].title + "_CheckboxButton" : "dummyTitle_CheckboxButton"} className="filter-button" onClick={() => onFilterClick()} content={< Text content={props.title} className={props.dir === LanguageDirection.Rtl ? "rtl-left-margin-small" : ""}/>} iconPosition="after" icon = {< ChevronDownIcon />} text />}
                content={<PopupMenuCheckboxesContent dir={props.dir} disableClear={disableClear} selectedCount={props.selectedCount} showSearchBar={props.showSearchBar!} content={{ checkboxes: props.checkboxes, title: props.title }} onCheckboxStateChange={props.onCheckboxStateChange} />}
                trapFocus
            />
        );
    }
    else if (props.radioGroup) {
        return (
            <Popup
                open={popup.isOpen}
                align="end"
                position="below"
                onOpenChange={(e, { open }: any) => onPopupOpenChange(open)}
                trigger={<Button data-testid={props.radioGroup.length > 0 ? props.radioGroup[0].name + "_RadioGroupButton" : "dummyName_RadioGroupButton"} icon={< ChevronDownIcon />} className="filter-button" iconPosition="after" content={< Text content={props.title} className={props.dir === LanguageDirection.Rtl ? "rtl-left-margin-small" : ""}/>} text />}
                content={<PopupMenuRadiogroupContent dir={props.dir} selectedValue={props.selectedSortBy!} content={{ radioGroupItems: props.radioGroup, title: props.title }} onRadiogroupStateChange={props.onRadiogroupStateChange} />}
                trapFocus
            />
        );
    }
    else {
        return (<></>);
    }
}

export default React.memo(PopupMenuWrapper);