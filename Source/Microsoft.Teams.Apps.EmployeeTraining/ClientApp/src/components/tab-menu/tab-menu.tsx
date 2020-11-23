// <copyright file="tab-menu.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Menu, MenuProps } from "@fluentui/react-northstar";
import "./tab-menu.css";

interface ITabMenuProps {
    defaultTabIndex: number,
    tabItems: Array<any>,
    activeTabIndex: string | number,
    onTabIndexChange: (tabEvent: MenuProps | undefined) => void
}

/**
 * Renders tab menu
 * @param props The props of type ITabMenuProps
 */
const TabMenu: React.FunctionComponent<ITabMenuProps> = props => {
    return (
        <Flex className="tab-menu">
            <Menu primary items={props.tabItems} defaultActiveIndex={props.defaultTabIndex} activeIndex={props.activeTabIndex} onActiveIndexChange={(event, tabEventDetails: MenuProps | undefined) => props.onTabIndexChange(tabEventDetails)} />
        </Flex>
    );
}

export default TabMenu;