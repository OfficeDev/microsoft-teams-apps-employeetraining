// <copyright file="filter-bar.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Text, Provider, Loader, Button } from "@fluentui/react-northstar";
import { initializeIcons } from "@uifabric/icons";
import AccordionMenuWrapper from "../../components/common/accordion-menu/accordion-menu-wrapper";
import { ICheckBoxItem } from "../../models/ICheckBoxItem";
import { IRadioGroupItem } from "../../models/IRadioGroupItem";
import { WithTranslation, withTranslation } from "react-i18next";
import withContext, { IWithContext } from "../../providers/context-provider";
import { TFunction } from "i18next";
import { getLocalizedSortBy } from "../../helpers/localized-constants";
import { ICategory } from "../../models/ICategory";
import { ITeamsChannelMember } from "../../models/ITeamsChannelMember";
import Resources from "../../constants/resources";
import { ResponseStatus } from "../../constants/constants";
import { getEventCategoriesAsync } from "../../api/create-event-api";
import { getAllLnDTeamMembersAsync } from "../../api/LnD-team-api";
import { SortBy } from "../../models/sort-by";

import "./mobile-filter-bar.css";

interface IMobileFilterBarState {
    categoryList: Array<ICheckBoxItem>;
    createdByList: Array<ICheckBoxItem>;
    selectedSortBy: number;
    searchText: string;
    screenWidth: number;
    createdBySelectedCount: number;
    categorySelectedCount: number;
    isLoading: boolean;
    isCategoriesFilterOpen: boolean,
    isCreatedByFilterOpen: boolean,
    isSortByFilterOpen: boolean
}

interface IMobileFilterBarProps extends WithTranslation, IWithContext  {
}

interface IPostType {
    name: string;
    id: number;
    color: string;
}

class MobileFilterBar extends React.Component<IMobileFilterBarProps, IMobileFilterBarState> {
    localize: TFunction;
    readonly sortByList: Array<IRadioGroupItem> = [];
    isFilterStateChanged: boolean;

    constructor(props) {
        super(props);

        initializeIcons();
        this.localize = this.props.t;
        this.sortByList = getLocalizedSortBy(this.localize).map((sortBy: IPostType) => { return { key: sortBy.id, label: sortBy.name, value: sortBy.id, name: sortBy.name } });
        this.isFilterStateChanged = false;

        this.state = {
            selectedSortBy: this.sortByList[0].value,
            categoryList: [],
            createdByList: [],
            searchText: "",
            screenWidth: 800,
            createdBySelectedCount: 0,
            categorySelectedCount: 0,
            isLoading: true,
            isCategoriesFilterOpen: false,
            isCreatedByFilterOpen: false,
            isSortByFilterOpen: false
        }
    }

    componentDidMount() {
        this.loadFilterItems();

        window.addEventListener("resize", this.resize.bind(this));
        this.resize();
    }

    componentWillUnmount() {
        window.removeEventListener("resize", this.resize.bind(this));
    }

    /** Get all event categories and load in filter */
    loadEventCategories = async () => {
        let response = await getEventCategoriesAsync();

        if (response && response.status === ResponseStatus.OK && response.data?.length > 0) {
            if (response && response.status === ResponseStatus.OK && response.data?.length > 0) {
                this.setState({
                    categoryList: response.data.map((value: ICategory) => {
                        return { isChecked: false, key: value.categoryId, title: value.name, checkboxLabel: <Text content={value.name} /> };
                    })
                });
            }
        }
    }

    /** Get all LnD teams' members and load in filter */
    loadAllLnDTeamMembers = async () => {
        let response = await getAllLnDTeamMembersAsync();

        if (response && response.status === ResponseStatus.OK && response.data?.length > 0) {
            if (response && response.status === ResponseStatus.OK && response.data?.length > 0) {
                this.setState({
                    createdByList: response.data.map((value: ITeamsChannelMember) => {
                        return { isChecked: false, key: value.aadObjectId, title: value.name, checkboxLabel: <Text content={value.name} /> };
                    })
                });
            }
        }
    }

    /** Populates the filter items */
    loadFilterItems = async () => {
        let eventCategories = this.loadEventCategories();
        let lndTeamMembers = this.loadAllLnDTeamMembers();

        Promise.all([eventCategories, lndTeamMembers])
            .finally(() => {
                let filteredCategories = localStorage.getItem(Resources.userEventsMobileFilteredCategoriesLocalStorageKey);
                let filteredUsers = localStorage.getItem(Resources.userEventsMobileFilteredUsersLocalStorageKey);
                let sortBy = localStorage.getItem(Resources.userEventsMobileSortByFilterLocalStorageKey);

                let categoryList = [...this.state.categoryList];
                let categorySelectedCount: number = 0;

                if (filteredCategories?.trim().length && categoryList?.length > 0) {
                    let categories = categoryList.filter((category: ICheckBoxItem) => filteredCategories && filteredCategories.indexOf(category.key) > -1);

                    for (let i = 0; i < categories.length; i++) {
                        categories[i].isChecked = true;
                        categorySelectedCount += 1;
                    }
                }

                let createdByList = [...this.state.createdByList];
                let createdBySelectedCount: number = 0;

                if (filteredUsers?.trim().length && createdByList?.length > 0) {
                    let users = createdByList.filter((user: ICheckBoxItem) => filteredUsers && filteredUsers.indexOf(user.key) > -1);

                    for (let i = 0; i < users.length; i++) {
                        users[i].isChecked = true;
                        createdBySelectedCount += 1;
                    }
                }

                let sortByFilter = this.state.selectedSortBy;

                if (sortBy?.trim().length) {
                    let sortByValue = this.sortByList?.find((sortByItem: IRadioGroupItem) => sortByItem.key.toString() === sortBy?.trim());

                    if (sortByValue) {
                        sortByFilter = sortByValue.key;
                    }
                }

                this.setState({
                    categoryList,
                    categorySelectedCount,
                    createdByList,
                    createdBySelectedCount,
                    selectedSortBy: sortByFilter,
                    isLoading: false
                });
            });
    }

    resize = () => {
        if (window.innerWidth !== this.state.screenWidth) {
            this.setState({ screenWidth: window.innerWidth });
        }
    }

	/**
	*Sets state of 'Category list' filter item when checkbox value changes.
	*@param categoryValues Array of 'category' checkboxes with updated user selection
    */
    onCategoryStateChange = (categoryValues: Array<ICheckBoxItem>) => {
        let selectedCategories = categoryValues?.filter((value: ICheckBoxItem) => {
            return value.isChecked;
        });

        this.setState({
            categoryList: categoryValues,
            categorySelectedCount: selectedCategories && selectedCategories.length
        }, () => this.isFilterStateChanged = true);
    }

	/**
	*Sets state of 'Created by' filter item when checkbox value changes.
	*@param createdByValues Array of 'created by' checkboxes with updated user selection
    */
    onCreatedByStateChange = (createdByValues: Array<ICheckBoxItem>) => {
        let selectedUsers = createdByValues?.filter((value: ICheckBoxItem) => {
            return value.isChecked;
        });

        this.setState({
            createdByList: createdByValues,
            createdBySelectedCount: selectedUsers ? selectedUsers.length : 0
        }, () => this.isFilterStateChanged = true);
    }

	/**
	*Sets state of selected sort by item.
	*@param selectedSortBy Selected 'sort by' value
    */
    onSortByStateChange = (selectedSortBy: number) => {
        this.setState({ selectedSortBy: selectedSortBy }, () => this.isFilterStateChanged = true);
    }

	/**
	*Removes all filters and hides filter bar.
	*@param event Event object for input
    */
    onCloseIconClick = () => {
        if (this.state.searchText.trim().length > 0) {
            this.setState({ searchText: "" });
        }

        if (this.state.categoryList.filter((sharedBy: ICheckBoxItem) => { return sharedBy.isChecked }).length) {
            let updatedList = this.state.categoryList.map((sharedBy: ICheckBoxItem) => { sharedBy.isChecked = false; return sharedBy; });
            this.setState({ categoryList: updatedList });
        }

        if (this.state.createdByList.filter((tag: ICheckBoxItem) => { return tag.isChecked }).length) {
            let updatedList = this.state.createdByList.map((tag: ICheckBoxItem) => { tag.isChecked = false; return tag; });
            this.setState({ createdByList: updatedList });
        }

        this.setState({ selectedSortBy: this.sortByList[0].value });
    }

    onCategoriesFilterOpenStateChange = (isOpen: boolean) => {
        if (isOpen) {
            this.setState({ isCategoriesFilterOpen: true, isCreatedByFilterOpen: false, isSortByFilterOpen: false });
        }
        else {
            this.setState({ isCategoriesFilterOpen: false });
        }
    }

    onCreatedByFilterOpenStateChange = (isOpen: boolean) => {
        if (isOpen) {
            this.setState({ isCategoriesFilterOpen: false, isCreatedByFilterOpen: true, isSortByFilterOpen: false });
        }
        else {
            this.setState({ isCreatedByFilterOpen: false });
        }
    }

    onSortByFilterOpenStateChange = (isOpen: boolean) => {
        if (isOpen) {
            this.setState({ isCategoriesFilterOpen: false, isCreatedByFilterOpen: false, isSortByFilterOpen: true });
        }
        else {
            this.setState({ isSortByFilterOpen: false });
        }
    }

    onClearFilter = () => {
        let categoryList = this.state.categoryList ? [...this.state.categoryList] : [];
        let createdByList = this.state.createdByList ? [...this.state.createdByList] : [];

        this.setState({
            categoryList: categoryList.map((value: ICheckBoxItem) => {
                value.isChecked = false;
                return value;
            }),
            categorySelectedCount: 0,
            createdByList: createdByList.map((value: ICheckBoxItem) => {
                value.isChecked = false;
                return value;
            }),
            createdBySelectedCount: 0,
            selectedSortBy: SortBy.Recent
        }, () => {
            this.isFilterStateChanged = true;
        });
    }

    onApplyFilter = () => {
        let categories: Array<ICheckBoxItem> = this.state.categoryList ? [...this.state.categoryList] : [];

        let selectedCategories = categories.filter((category: ICheckBoxItem) => {
            return category.isChecked;
        });

        let selectedCategoryIds: Array<string> = selectedCategories.map((category: ICheckBoxItem) => { return category.key });
        localStorage.setItem(Resources.userEventsMobileFilteredCategoriesLocalStorageKey, selectedCategoryIds.join(";"));

        let users: Array<ICheckBoxItem> = this.state.createdByList ? [...this.state.createdByList] : [];

        let selectedUsers = users.filter((user: ICheckBoxItem) => {
            return user.isChecked;
        });

        let selectedUserIds: Array<string> = selectedUsers.map((user: ICheckBoxItem) => { return user.key });
        localStorage.setItem(Resources.userEventsMobileFilteredUsersLocalStorageKey, selectedUserIds.join(";"));

        localStorage.setItem(Resources.userEventsMobileSortByFilterLocalStorageKey, this.state.selectedSortBy.toString());

        this.props.microsoftTeams.tasks.submitTask({ isFilterStateChanged: this.isFilterStateChanged });
    }

	/**
	* Renders the component
	*/
    public render(): JSX.Element {
        if (this.state.isLoading) {
            return <Provider><Loader className="loader" /></Provider>
        }

        return (
            <Provider className="mobile-filter-bar-container">
                <Flex className="mobile-filter-buttons">
                    <Flex.Item push>
                        <Flex gap="gap.small" vAlign="center">
                            <Button content={this.localize("mobileFilterClearButtonText")} onClick={this.onClearFilter} />
                            <Button primary content={this.localize("mobileFilterApplyButtonText")} onClick={this.onApplyFilter} />
                        </Flex>
                    </Flex.Item>
                </Flex>
                <Flex className="filter-bar mobile-filter-wrapper">
                    <div className="menu-wrapper">
                        <AccordionMenuWrapper isFilterOpen={this.state.isCategoriesFilterOpen} title={`${this.localize("category")} ${this.state.categorySelectedCount > 0 ? `(${this.state.categorySelectedCount} ${this.localize("selected")})` : ""}`} showSearchBar={true} selectedSortBy={this.state.selectedSortBy!} selectedCount={this.state.categorySelectedCount} checkboxes={this.state.categoryList} onRadiogroupStateChange={this.onSortByStateChange} onCheckboxStateChange={this.onCategoryStateChange} onFilterOpenChange={this.onCategoriesFilterOpenStateChange} />
                        <AccordionMenuWrapper isFilterOpen={this.state.isCreatedByFilterOpen} title={`${this.localize("createdBy")} ${this.state.createdBySelectedCount > 0 ? `(${this.state.createdBySelectedCount} ${this.localize("selected")})` : ""}`} showSearchBar={true} selectedSortBy={this.state.selectedSortBy!} selectedCount={this.state.createdBySelectedCount} checkboxes={this.state.createdByList} onRadiogroupStateChange={this.onSortByStateChange} onCheckboxStateChange={this.onCreatedByStateChange} onFilterOpenChange={this.onCreatedByFilterOpenStateChange} />
                        <AccordionMenuWrapper isFilterOpen={this.state.isSortByFilterOpen} title={this.sortByList[this.state.selectedSortBy!].name} selectedCount={this.state.createdBySelectedCount} selectedSortBy={this.state.selectedSortBy!} radioGroup={this.sortByList} onRadiogroupStateChange={this.onSortByStateChange} onCheckboxStateChange={this.onCreatedByStateChange} onFilterOpenChange={this.onSortByFilterOpenStateChange} />
                    </div>
                </Flex>
            </Provider>
        );
    }
}

export default withTranslation()(withContext(MobileFilterBar));