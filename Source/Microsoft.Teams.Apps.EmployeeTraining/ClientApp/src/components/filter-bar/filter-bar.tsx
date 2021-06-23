// <copyright file="filter-bar.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Text } from "@fluentui/react-northstar";
import { CloseIcon } from "@fluentui/react-icons-northstar";
import { initializeIcons } from "@uifabric/icons";
import PopupMenuWrapper from "../../components/common/popup-menu/popup-menu-wrapper";
import { ICheckBoxItem } from "../../models/ICheckBoxItem";
import { IRadioGroupItem } from "../../models/IRadioGroupItem";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { getLocalizedSortBy } from "../../helpers/localized-constants";
import { ICategory } from "../../models/ICategory";
import { ITeamsChannelMember } from "../../models/ITeamsChannelMember";
import { SortBy } from "../../models/sort-by";
import withContext, { IWithContext } from "../../providers/context-provider";
import { Fabric } from "@fluentui/react";
import { LanguageDirection } from "../../models/language-direction";

import "./filter-bar.css";

interface IFilterBarProps extends WithTranslation, IWithContext {
    isVisible: boolean,
    isReset: boolean,
    categoryList: Array<ICategory>,
    createdByList: Array<ITeamsChannelMember>,
    onFilterBarCloseClick: (isFilterStateChanged: boolean) => void,
    onFilterChange: (selectedCategories: Array<string>, selectedUsers: Array<string>, sortBy: number) => void
}

interface IFilterBarState {
    categoryList: Array<ICheckBoxItem>;
    createdByList: Array<ICheckBoxItem>;
    sortBy: Array<IRadioGroupItem>;
    selectedSortBy: number;
    screenWidth: number;
    createdBySelectedCount: number;
    categorySelectedCount: number;
}

interface IPostType {
    name: string;
    id: number;
    color: string;
}

class FilterBar extends React.Component<IFilterBarProps, IFilterBarState> {
    localize: TFunction;
    isFilterStateChanged: boolean;

    constructor(props: IFilterBarProps) {
        super(props);

        initializeIcons();
        this.localize = this.props.t;
        this.isFilterStateChanged = false;

        const sortBy: Array<IRadioGroupItem> = getLocalizedSortBy(this.localize).map((sortBy: IPostType) => { return { key: sortBy.id, label: sortBy.name, value: sortBy.id, name: sortBy.name } });

        this.state = {
            selectedSortBy: sortBy[0].value,
            categoryList: [],
            createdByList: [],
            sortBy: sortBy,
            screenWidth: 800,
            createdBySelectedCount: 0,
            categorySelectedCount: 0
        }
    }

    componentDidMount() {
        window.addEventListener("resize", this.resize.bind(this));
        this.resize();
    }

    componentWillUnmount() {
        window.removeEventListener("resize", this.resize.bind(this));
    }

    componentWillReceiveProps(nextProps: IFilterBarProps) {
        if (nextProps.isReset) {
            let categoryList = this.state.categoryList ? [...this.state.categoryList] : [];
            let createdByList = this.state.createdByList ? [...this.state.createdByList] : [];

            this.isFilterStateChanged = false;

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
            });
        }

        if (nextProps.categoryList && nextProps.categoryList !== this.props.categoryList) {
            this.setState({
                categoryList: nextProps.categoryList.map((value: ICategory) => {
                    return { isChecked: false, key: value.categoryId, title: value.name, checkboxLabel: <Text content={value.name} /> };
                })
            });
        }

        if (nextProps.createdByList && nextProps.createdByList !== this.props.createdByList) {
            this.setState({
                createdByList: nextProps.createdByList.map((value: ITeamsChannelMember) => {
                    return { isChecked: false, key: value.aadObjectId, title: value.name, checkboxLabel: <Text content={value.name} /> };
                })
            });
        }
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

        this.isFilterStateChanged = true;
        this.setState({ categoryList: categoryValues, categorySelectedCount: selectedCategories ? selectedCategories.length : 0 });
    }

	/**
	*Sets state of 'Created by' filter item when checkbox value changes.
	*@param createdByValues Array of 'created by' checkboxes with updated user selection
    */
    onCreatedByStateChange = (createdByValues: Array<ICheckBoxItem>) => {
        let selectedUsers = createdByValues?.filter((value: ICheckBoxItem) => {
            return value.isChecked;
        });

        this.isFilterStateChanged = true;
        this.setState({ createdByList: createdByValues, createdBySelectedCount: selectedUsers ? selectedUsers.length : 0 });
    }

	/**
	*Sets state of selected sort by item.
	*@param selectedSortBy Selected 'sort by' value
    */
    onSortByStateChange = (selectedSortBy: number) => {
        this.isFilterStateChanged = true;
        this.setState({ selectedSortBy: selectedSortBy });
    }

	/**
	*Removes all filters and hides filter bar.
	*@param event Event object for input
    */
    onCloseIconClick = (event: any) => {
        this.props.onFilterBarCloseClick(this.state.categorySelectedCount > 0 || this.state.createdBySelectedCount > 0 || this.state.selectedSortBy !== SortBy.Recent );
        this.isFilterStateChanged = false;
    }

    onFilterChange = (isFilterItemOpen: boolean) => {
        if (!isFilterItemOpen && this.isFilterStateChanged) {
            let categories: Array<ICheckBoxItem> = this.state.categoryList ? [...this.state.categoryList] : [];
            let users: Array<ICheckBoxItem> = this.state.createdByList ? [...this.state.createdByList] : [];

            let selectedCategories = categories.filter((category: ICheckBoxItem) => {
                return category.isChecked;
            });

            let selectedUsers = users.filter((user: ICheckBoxItem) => {
                return user.isChecked;
            });

            let filteredCategories = selectedCategories.map((category: ICheckBoxItem) => {
                return category.key;
            });

            let filteredUsers = selectedUsers.map((user: ICheckBoxItem) => {
                return user.key;
            });

            this.isFilterStateChanged = false;
            this.props.onFilterChange(filteredCategories, filteredUsers, this.state.selectedSortBy);
        }
        else {
            this.isFilterStateChanged = false;
        }
    }

	/**
	* Renders the component
	*/
    public render(): JSX.Element {
        if (this.props.isVisible) {
            return (
                <Fabric dir={this.props.dir}>
                    <Flex className="filter-bar" data-testid="filterbar">
                        {this.state.screenWidth > 750 &&
                            <Flex gap="gap.small" vAlign="center" className="filter-bar-wrapper" space="between">
                                <div className="filter-bar-item-container">
                                    <Text content={this.localize("category")} weight="semibold" className="title-text"/>
                                    <PopupMenuWrapper dir={this.props.dir} title={this.state.categorySelectedCount > 0 ? this.state.categorySelectedCount + " " + this.localize("selected") : this.localize("selectHere")} showSearchBar={true} selectedSortBy={this.state.selectedSortBy!} selectedCount={this.state.categorySelectedCount} checkboxes={this.state.categoryList} onRadiogroupStateChange={this.onSortByStateChange} onCheckboxStateChange={this.onCategoryStateChange} onOpenChange={this.onFilterChange} />
                                        <Text content={this.localize("createdBy")} weight="semibold" className="title-text"/>
                                    <PopupMenuWrapper dir={this.props.dir} title={this.state.createdBySelectedCount > 0 ? this.state.createdBySelectedCount + " " + this.localize("selected") : this.localize("selectHere")} showSearchBar={true} selectedSortBy={this.state.selectedSortBy!} checkboxes={this.state.createdByList} selectedCount={this.state.createdBySelectedCount} onRadiogroupStateChange={this.onSortByStateChange} onCheckboxStateChange={this.onCreatedByStateChange} onOpenChange={this.onFilterChange} />
                                        <Text content={this.localize("sortBy")} weight="semibold" className="title-text"/>
                                    <PopupMenuWrapper dir={this.props.dir} title={this.state.sortBy[this.state.selectedSortBy!].name} selectedSortBy={this.state.selectedSortBy!} selectedCount={this.state.createdBySelectedCount} radioGroup={this.state.sortBy} onRadiogroupStateChange={this.onSortByStateChange} onCheckboxStateChange={this.onCreatedByStateChange} onOpenChange={this.onFilterChange} />
                                </div>
                            <Flex.Item push={this.props.dir === LanguageDirection.Ltr}>
                                <CloseIcon onClick={this.onCloseIconClick} className={this.props.dir === LanguageDirection.Rtl ? "close-icon rtl-left-margin-small" : "close-icon"} />
                                </Flex.Item>
                            </Flex>}

                        {this.state.screenWidth <= 750 && <Flex gap="gap.small" vAlign="start" className="filter-bar-wrapper">
                            <Flex.Item grow>
                                <Flex column gap="gap.small" vAlign="stretch">
                                    <Flex className="mobile-filterbar-wrapper">
                                        <PopupMenuWrapper dir={this.props.dir} title={this.state.categorySelectedCount > 0 ? this.state.categorySelectedCount + " " + this.localize("selected") : this.localize("selectHere")} showSearchBar={true} selectedSortBy={this.state.selectedSortBy!} selectedCount={this.state.categorySelectedCount} checkboxes={this.state.categoryList} onRadiogroupStateChange={this.onSortByStateChange} onCheckboxStateChange={this.onCategoryStateChange} onOpenChange={this.onFilterChange} />
                                        <PopupMenuWrapper dir={this.props.dir} title={this.state.createdBySelectedCount > 0 ? this.state.createdBySelectedCount + " " + this.localize("selected") : this.localize("selectHere")} showSearchBar={true} selectedSortBy={this.state.selectedSortBy!} selectedCount={this.state.createdBySelectedCount} checkboxes={this.state.createdByList} onRadiogroupStateChange={this.onSortByStateChange} onCheckboxStateChange={this.onCreatedByStateChange} onOpenChange={this.onFilterChange} />
                                        <PopupMenuWrapper dir={this.props.dir} title={this.state.sortBy[this.state.selectedSortBy!].name} selectedCount={this.state.createdBySelectedCount} selectedSortBy={this.state.selectedSortBy!} radioGroup={this.state.sortBy} onRadiogroupStateChange={this.onSortByStateChange} onCheckboxStateChange={this.onCreatedByStateChange} onOpenChange={this.onFilterChange} />
                                    </Flex>
                                </Flex>
                            </Flex.Item>
                        </Flex>}
                    </Flex>
                </Fabric>
            );
        }
        else {
            return (<></>);
        }
    }
}

export default withTranslation()(withContext(FilterBar))