import React, { useState, useEffect, useContext } from 'react';
import ChatWindow from '../chatWindow';
import HubContext from '../../contexts/hubContext';
import './groupChat.scss';



export default function GroupChat(props) {

    const { user, messages, hubConnection } = useContext(HubContext);

    const [message, setMessage] = useState("");

    const [windowMessages, setWindowMessages] = useState([]);

    const [messageCount, setMessageCount] = useState(0);

    const channelName = props.name;

    useEffect(() => {
        let newMessages = messages.filter(msg => msg.recipient === channelName);
        setWindowMessages(newMessages);
    }, [messages, channelName]);

    useEffect(() => {
        setMessageCount(windowMessages.length);
    }, [windowMessages]);

    useEffect(() => {
        connectToGroup();
    });

    const connectToGroup = async () => {
        if (hubConnection.connectionStarted) {
            console.log("Connect to group!");
            await hubConnection.invoke("AddToGroup", channelName).catch(function (err) {
                return console.error(err.toString());
            });
        }
    }
    const submitHandler = async (e) => {
        e.preventDefault();
        let thisForm = e.target;

        if (hubConnection.connectionStarted) {
            console.log("sending!")
            await hubConnection.invoke("SendGroupMessage", user, channelName, message).catch(function (err) {
                return console.error(err.toString());
            });
        }
        else {
            alert('No connection to server yet.');
        }
        thisForm.reset();
    };

    const changeHandler = e => {
        setMessage(e.target.value);
    };

    return (
        <div className="Chat" >
            <ChatWindow messages={windowMessages} count={messageCount} />
            <form onSubmit={submitHandler}>
                <label>
                    <input type="text" name="name" onChange={changeHandler} />
                </label>
                <button name="name" type="submit">Send</button>
            </form>
        </div>
    )
}