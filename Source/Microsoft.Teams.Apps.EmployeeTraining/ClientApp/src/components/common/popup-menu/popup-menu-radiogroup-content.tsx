// <copyright file="popup-menu-radiogroup-content.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, RadioGroup } from "@fluentui/react-northstar";

import "./popup-menu.css";

interface IPopupMenuRadiogroupContentProps {
    content: any,
    selectedValue: number,
    onRadiogroupStateChange: (selectedValue: number) => void
}

const PopupMenuRadiogroupContent: React.FunctionComponent<IPopupMenuRadiogroupContentProps> = props => {

    return (
        <>
            <div className="content-items-wrapper radio-popup-content">
                <div className="content-items-body">
                    {   
                        <Flex gap="gap.small">
				            <RadioGroup
					            defaultCheckedValue={props.selectedValue}
					            vertical
					            items={props.content.radioGroupItems}
                                onCheckedValueChange={(event, data: any) => props.onRadiogroupStateChange(data.value)}
                                data-testid={"radioGroup_items"}
				            />
			            </Flex>
                    }
                </div>
            </div>
        </>
    );
}

export default React.memo(PopupMenuRadiogroupContent);