// <copyright file="popup-menu-checkboxes-content.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Input, Button, Provider, Divider, Checkbox } from "@fluentui/react-northstar";
import { CloseIcon, SearchIcon } from "@fluentui/react-icons-northstar";
import { useTranslation } from 'react-i18next';
import { ICheckBoxItem } from "../../../models/ICheckBoxItem";
import { LanguageDirection } from "../../../models/language-direction";
import { Fabric } from "@fluentui/react";

import "./popup-menu.css";

interface IPopupMenuCheckboxesContentProps {
    showSearchBar: boolean,
    content: any,
    disableClear: boolean,
    selectedCount: number,
    onCheckboxStateChange: (checkboxState: Array<ICheckBoxItem>) => void,
    dir: LanguageDirection
}

const MaxItemsToShowInFilter: number = 50;

const PopupMenuCheckboxesContent: React.FunctionComponent<IPopupMenuCheckboxesContentProps> = props => {
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
        let checkCount = 0;
        setcheckBoxClicked(true);
        let checkboxList = data.checkboxes.map((checkbox: ICheckBoxItem) => {
            if (checkbox.key === key) {
                checkbox.isChecked = checked;
            }
            return checkbox;
        });

        checkboxList.map((checkbox: ICheckBoxItem) => {
            if (checkbox.isChecked) {
                checkCount = checkCount + 1;
            }
        })

        if (checkCount > 0) {
            setdisableClear(false);
        }
        else {
            setdisableClear(true);
        }

        if (searchedString.trim().length) {
            let filteredItems = checkboxList.filter((item: ICheckBoxItem) => {
                return item.title.toLowerCase().includes(searchedString.toLowerCase());
            })

            setFilterCheckboxes(filteredItems);
        }
        else {
            setFilterCheckboxes(checkboxList);
        }

        props.onCheckboxStateChange(checkboxList);
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
                return item.title.toLowerCase().includes(text.toLowerCase());
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

    const setFilterCheckboxes = (filterItems: any) => {
        if (filterItems && filterItems.length > 0) {
            let items = [...filterItems];
            let itemsToRender = items.slice(0, MaxItemsToShowInFilter);
            setFilteredCheckboxes(itemsToRender);
        }
        else {
            setFilteredCheckboxes([]);
        }
    }

    return (
        <Fabric dir={props.dir}>
            <Provider>
                <div className="content-items-wrapper">
                    {props.showSearchBar && <div className="content-items-headerfooter">
                        <Input icon={<SearchIcon />} iconPosition={props.dir === "rtl" ? "start" : "end" } placeholder={localize("searchPlaceholder")} value={searchedString} fluid onChange={(event: any) => onSearchChange(event.target.value)} />
                    </div>}
                    <Divider className="filter-popup-menu-divider" />
                    <div className="content-items-headerfooter">
                        <Flex gap="gap.small" vAlign="center" hAlign="end">
                            <Flex.Item push>
                                <div></div>
                            </Flex.Item>
                            <Button disabled={checkBoxClicked ? disableClear : props.disableClear} className={props.selectedCount === 0 ? "clear-button ": "clear-button enable-clear"} size="small" text onClick={() => deSelectAll()} content={props.selectedCount > 0 ? localize("clear") + "(" + getSelectedCountString() + ")" : localize("clear") } />
                        </Flex>
                    </div>
                    <div className="content-items-body">
                        {
                            filteredCheckboxes.map((checkbox: ICheckBoxItem) => {
                                if (checkbox.title.trim().length) {
                                    return (
                                        <Flex gap="gap.small">
                                            <Checkbox data-testid={checkbox.key + "_categoryCheckbox_item"} className="checkbox-wrapper" label={checkbox.checkboxLabel} key={checkbox.key} checked={checkbox.isChecked} onChange={(key, data: any) => onCheckboxValueChange(checkbox.key, data.checked)} />
                                        </Flex>
                                    );
                                }
                            })
                        }
                    </div>
                </div>
            </Provider>
        </Fabric>
    );
}

export default React.memo(PopupMenuCheckboxesContent);