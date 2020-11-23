// <copyright file="dropdown-search.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Dropdown } from '@fluentui/react-northstar'
import { searchUsersAndGroups } from "../../../api/user-group-api";

interface IDropdownProps {
    onItemSelect: (selectedItem: IDropdownItem) => void,
    loadingMessage: string,
    placeholder: string,
    noResultMessage: string
}

export interface IDropdownItem {
    header: string,
    content: string,
    id: string,
    email: string,
    isGroup: boolean
}

interface IUserOrGroupSearchResult {
    displayName: string,
    id: string,
    email: string,
    isGroup: boolean
}

const DropdownSearch: React.FunctionComponent<IDropdownProps> = props => {
    let timeout: number | null = null; // to handle API call on user input
    let initialReults = new Array<IDropdownItem>();

    const [searchResult, setSearchResult] = React.useState(new Array<IDropdownItem>());
    const [loading, setLoading] = React.useState(true);
    const [isOpen, setOpen] = React.useState(false);
    const [key, setKey] = React.useState(0);
    const [selectedValue, setselectedValue] = React.useState(undefined);

    const searchUsingAPI = async (searchQuery: string) => {
        if (initialReults.length && !searchQuery.length) {
            setSearchResult(initialReults);
            return;
        }

        let array = new Array<IDropdownItem>();
        const response = await searchUsersAndGroups(searchQuery);
        if (response.status === 200 && response.data) {
            const results: Array<IUserOrGroupSearchResult> = response.data;
            for (let i = 0; i < results.length; i++) {
                if (results[i].displayName && results[i].email) {
                    array.push({ header: results[i].displayName, content: results[i].email, id: results[i].id, email: results[i].email, isGroup: results[i].isGroup });
                }
            }
        }

        if (!initialReults.length) {
            initialReults = [...array];
        }

        setSearchResult(array);
        setLoading(false);
    }

    const initiateSearch = (searchQuery: string) => {
        if (timeout) {
            window.clearTimeout(timeout);
        }
        if (!loading) {
            setLoading(true);
        }

        timeout = window.setTimeout(async () => { await searchUsingAPI(searchQuery) }, 750);
    }

    const onTypeSelection = {
        onAdd: (item: any) => {
            props.onItemSelect(item);
            setKey(key + 1);
            return "";
        },
    };

    return (
        <Dropdown
            search
            styles={{ width: "100%" }}
            fluid
            key={key}
            loading={loading}
            loadingMessage={props.loadingMessage}
            items={searchResult}
            value={selectedValue}
            onChange={(e, { value }) => {
                setselectedValue(undefined);
            }}
            onSearchQueryChange={(e, { searchQuery }) => {
                initiateSearch(searchQuery!);
            }}
            onOpenChange={(e, { open, value }) => {
                if (open) {
                    setLoading(true);
                    searchUsingAPI("");
                }
                else {
                    setSearchResult(new Array<IDropdownItem>())
                }
                setOpen(open!);
            }}
            open={isOpen}
            getA11ySelectionMessage={onTypeSelection}
            placeholder={props.placeholder}
            noResultsMessage={props.noResultMessage}
            data-testid="audience_dropdown_search"
        />
    )
}

export default DropdownSearch;