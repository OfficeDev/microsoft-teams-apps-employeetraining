// <copyright file="accordion-menu-wrapper.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Text, Flex } from "@fluentui/react-northstar";
import { ChevronDownIcon } from "@fluentui/react-icons-northstar";
import AccordionCheckboxesContent from "./accordion-menu-checkboxes-content";
import AccordionRadiogroupContent from "./accordion-menu-radiogroup-content";

import "./accordion-menu.css";

interface IAccordionMenuWrapperProps {
    isFilterOpen: boolean,
    checkboxes?: Array<any>,
    radioGroup?: Array<any>,
    title: string,
    selectedSortBy?: number,
    showSearchBar?: boolean,
    selectedCount: number;
    onCheckboxStateChange: (typeState: Array<any>) => void,
    onRadiogroupStateChange: (selectedValue: number) => void,
    onFilterOpenChange: (isOpen: boolean) => void
}

const AccordionMenuWrapper: React.FunctionComponent<IAccordionMenuWrapperProps> = props => {
    const [open, setOpen] = React.useState(false);

    const onAccordionOpenChange = () => {
        setOpen(!open);
        props.onFilterOpenChange(!open);
    }

    if (props.checkboxes) {
        return (
            <div className="accordion-container">
                <div className="accordion-main" onClick={() => onAccordionOpenChange()}>
                    <Flex><Flex.Item><Text content={props.title} /></Flex.Item><Flex.Item push><ChevronDownIcon /></Flex.Item></Flex>
                </div>
                <AccordionCheckboxesContent isOpen={props.isFilterOpen} disableClear={props.selectedCount === 0} selectedCount={props.selectedCount} showSearchBar={props.showSearchBar!} content={{ checkboxes: props.checkboxes, title: props.title }} onCheckboxStateChange={props.onCheckboxStateChange} />
            </div>
        );
    }
    else if (props.radioGroup) {
        return (
            <div className="accordion-container">
                <div className="accordion-main" onClick={() => onAccordionOpenChange()}>
                    <Flex><Flex.Item><Text content={props.title} /></Flex.Item><Flex.Item push><ChevronDownIcon /></Flex.Item></Flex>
                </div>
                <AccordionRadiogroupContent isOpen={props.isFilterOpen} selectedValue={props.selectedSortBy!} content={{ radioGroupItems: props.radioGroup, title: props.title }} onRadiogroupStateChange={props.onRadiogroupStateChange} />
            </div>
        );
    }
    else {
        return (<></>);
    }
}

export default React.memo(AccordionMenuWrapper);