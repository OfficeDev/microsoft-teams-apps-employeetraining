// <copyright file="user-events-wrapper.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Container, Row, Col } from 'react-bootstrap';
import { Loader } from "@fluentui/react-northstar";
import InfiniteScroll from "react-infinite-scroller";
import EventCard from "../../components/event-card/event-card";
import { IEvent } from "../../models/IEvent";

import 'bootstrap/dist/css/bootstrap.min.css';
import "../../components/user-events-wrapper/user-events-wrapper.css"

interface IUserEventsProps {
    key: string,
    events: Array<IEvent>,
    hasMoreEvents: boolean,
    loadMoreEvents: (pageNumber: number) => void,
    onClick: (eventDetails: IEvent) => void
}

/**
 * Renders the events for the user
 * @param props The props of type IUserEvents
 */
const UserEvents: React.FunctionComponent<IUserEventsProps> = props => {
    /** Renders events details for every event */
    const renderEvents = () => {
        if (!props.events || props.events.length === 0) {
            return "No events available";
        }

        let events = props.events.map((event: IEvent, index: number) => {
            return <Col className="grid-column d-flex justify-content-center" xl={4} lg={4} sm={6} md={4}><EventCard key={`event-${index}`} eventDetails={event} onClick={() => props.onClick(event)} /></Col>
        });

        return <Row data-testid="event_count">{events}</Row>;
    }

    return (
        <div className="user-events">
            <div className="container-subdiv-cardview">
                <Container key={props.key} className="pagination-scroll-area" fluid>
                    <InfiniteScroll
                        pageStart={0}
                        initialLoad={false}
                        loader={<Loader />}
                        useWindow={false}
                        loadMore={props.loadMoreEvents}
                        hasMore={props.hasMoreEvents}
                    >
                        {renderEvents()}
                    </InfiniteScroll>
                </Container>
            </div>
        </div>
    );
}

export default UserEvents;