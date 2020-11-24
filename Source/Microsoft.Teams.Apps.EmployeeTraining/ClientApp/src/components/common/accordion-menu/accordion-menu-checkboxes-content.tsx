// <copyright file="accordion-menu-checkboxes-content.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Input, Button, Provider, Divider, Checkbox } from "@fluentui/react-northstar";
import { SearchIcon } from "@fluentui/react-icons-northstar";
import { useTranslation } from 'react-i18next';
import { ICheckBoxItem } from "../../../models/ICheckBoxItem";

import "./accordion-menu.css";

interface IAccordionMenuCheckboxesContentProps {
    isOpen: boolean,
    showSearchBar: boolean,
    content: any,
    disableClear: boolean,
    selectedCount: number;
    onCheckboxStateChange: (checkboxState: Array<ICheckBoxItem>) => void
}

const MaxItemsToShowInFilter: number = 50;

const AccordionCheckboxesContent: React.FunctionComponent<IAccordionMenuCheckboxesContentProps> = props => {
    const localize = useTranslation().t;
    const [data, setCheckbox] = React.useState({ checkboxes: props.content.checkboxes });
    const [filteredCheckboxes, setFilteredCheckboxes] = React.useState(props.content.checkboxes);
    const [searchedString, setSearchString] = React.useState("");
    let [disableClear, setdisableClear] = React.useState(true);
    let [checkBoxClicked, setcheckBoxClicked] = React.useState(false);

    React.useEffect(() => {
        setFilterCheckboxes(props.content.checkboxes);
    }, []);

    /**
    *Updates particular checkbox's isChecked state and passes changed state back to parent component.
    *@param key Unique key for checkbox which needs to be updated
    *@param checked Boolean indicating checkbox current value
    */
    const onCheckboxValueChange = (key: string, checked: boolean) => {
        setcheckBoxClicked(true);

        let checkboxList = data.checkboxes.map((checkbox: ICheckBoxItem) => {
            if (checkbox.key === key) {
                checkbox.isChecked = checked;
            }
            return checkbox;
        });

        let checkCount: number = checkboxList.reduce((accumulator: number, checkbox: ICheckBoxItem) => {
            return checkbox.isChecked ? accumulator + 1 : accumulator;
        }, 0);

        setdisableClear(checkCount <= 0);

        let filteredItems = checkboxList;

        if (searchedString.trim().length) {
            filteredItems = checkboxList.filter((item: ICheckBoxItem) => {
                return item.title.toLowerCase().includes(searchedString.toLowerCase());
            });
        }

        setFilterCheckboxes(filteredItems);
        props.onCheckboxStateChange(checkboxList);
    }

    const setFilterCheckboxes = (filterItems: any) => {
        if (!filterItems) {
            filterItems = [];
        }

        let items = [...filterItems];
        let itemsToRender = items.slice(0, MaxItemsToShowInFilter);
        setFilteredCheckboxes(itemsToRender);
    }

    /**
    *Sets all checkbox's isChecked to false to unselect all and passes changed state back to parent component.
    */
    const deSelectAll = () => {
        let checkboxList = data.checkboxes.map((checkbox: ICheckBoxItem) => {
            checkbox.isChecked = false
            return checkbox;
        });

        let filteredItems = checkboxList;

        if (searchedString.trim().length) {
            filteredItems = checkboxList.filter((item: ICheckBoxItem) => {
                return item.title.toLowerCase().includes(searchedString.toLowerCase());
            });
        }

        setFilterCheckboxes(filteredItems);
        props.onCheckboxStateChange(checkboxList);
        setdisableClear(true);
        setcheckBoxClicked(true);
    }

    const onSearchChange = (text: string) => {
        setSearchString(text);

        if (text.trim().length > 0) {
            let filteredItems = data.checkboxes.filter((item: ICheckBoxItem) => {
                return item.title.toLowerCase().includes(text.toLowerCase()) === true;
            });

            setFilterCheckboxes(filteredItems);
        }
        else {
            setFilterCheckboxes(data.checkboxes);
        }
    }

    const getSelectedCountString = () => {
            return props.selectedCount.toString();
    }

    return (
        props.isOpen ? 
            <Provider>
                <div className="accordion-content-items-wrapper">
                    {props.showSearchBar && <div className="accordion-content-items-headerfooter">
                        <Input icon={<SearchIcon />} placeholder={localize("searchPlaceholder")} value={searchedString} fluid onChange={(event: any) => onSearchChange(event.target.value)} />
                    </div>}
                    <Divider className="accordion-filter-popup-menu-divider" />
                    <div className="accordion-content-items-headerfooter">
                        <Flex gap="gap.small" vAlign="center" hAlign="end">
                            <Flex.Item push>
                                <div></div>
                            </Flex.Item>
                            <Button disabled={checkBoxClicked ? disableClear : props.disableClear} className={props.selectedCount === 0 ? "clear-button " : "clear-button enable-clear"} size="small" text onClick={() => deSelectAll()} content={props.selectedCount > 0 ? localize("clear") + "(" + getSelectedCountString() + ")" : localize("clear")} />
                        </Flex>
                    </div>
                    <div className="accordion-content-items-body">
                        {
                            filteredCheckboxes.map((checkbox: ICheckBoxItem) => {
                                if (checkbox.title.trim().length) {
                                    return (
                                        <Flex gap="gap.small">
                                            <Checkbox className="checkbox-wrapper" label={checkbox.checkboxLabel} key={checkbox.key} checked={checkbox.isChecked} onChange={(key, data: any) => onCheckboxValueChange(checkbox.key, data.checked)} />
                                        </Flex>
                                    );
                                }
                            })
                        }
                    </div>
                </div>
            </Provider>
            : <></>
    );
}

export default React.memo(AccordionCheckboxesContent);