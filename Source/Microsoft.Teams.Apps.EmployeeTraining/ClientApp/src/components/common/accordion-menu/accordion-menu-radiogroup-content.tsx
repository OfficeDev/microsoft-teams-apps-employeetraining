// <copyright file="popup-menu-radiogroup-content.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, RadioGroup } from "@fluentui/react-northstar";

import "./accordion-menu.css";

interface IAccordionMenuRadiogroupContentProps {
    isOpen: boolean,
    content: any,
    selectedValue: number,
    onRadiogroupStateChange: (selectedValue: number) => void
}

const AccordionRadiogroupContent: React.FunctionComponent<IAccordionMenuRadiogroupContentProps> = props => {

    return (
        props.isOpen ?
        <>
            <div className="accordion-content-items-wrapper radio-popup-content">
                <div className="accordion-content-items-body">
                    {   
                        <Flex gap="gap.small">
				            <RadioGroup
					            defaultCheckedValue={props.selectedValue}
					            vertical
					            items={props.content.radioGroupItems}
					            onCheckedValueChange={(event, data: any) => props.onRadiogroupStateChange(data.value)}
				            />
			            </Flex>
                    }
                </div>
            </div>
        </>
        : <></>
    );
}

export default React.memo(AccordionRadiogroupContent);