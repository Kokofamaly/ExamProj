import { useMutation, useQuery } from "@tanstack/react-query";
import { startTransition, useContext, useEffect, useOptimistic, useRef, useState, useTransition } from "react";
import "./Sessions.css";
import { apiFetch } from "./api/apiFetch";
import { UserContext } from "./UserContext";
import { jsx } from "react/jsx-runtime";
import { data } from "react-router-dom";

interface Session{
    id: string,
    createdAt: string,
    language?: string,
    category?: string

}

interface SessionCardProps{
    session: Session,
    setSessionList: React.Dispatch<React.SetStateAction<Session[]>>,
    setOptimisticSessionList: (action: Session[] | ((pendingState: Session[]) => Session[])) => void,
    setStartedSessionId: React.Dispatch<React.SetStateAction<string | null>>,
    startedSessionId: string | null
}


interface SessionWord{
    id: string,
    sessionId: string,
    userWordId: string,
    isCorrect: boolean | null,
    word: string,
    translation: string,
    usageExample?: string,
    order: number
}

interface StartedSessionProps{
    session: Session,
    sessionWords: Array<SessionWord>,
    startedSessionId: string,
    setStartedSessionId: React.Dispatch<React.SetStateAction<string | null>>
}

export function Sessions(){
    const user = useContext(UserContext);
    const [sessionList, setSessionList] = useState<Array<Session>>([]);
    const [optimisticSessionList, setOptimisticSessionList] = useOptimistic(sessionList);
    const [isAdding, setIsAdding] = useState(false);
    const [newSession, setNewSession] = useState<Omit<Session, "id" | "createdAt">>({language: "", category: ""});
    const [isPending, startTransition] = useTransition();
    const [startedSessionId, setStartedSessionId] = useState<string | null>(null);

    const sessionDrawerRef = useRef<HTMLDialogElement | null>(null);


    const getSessionsQuery = useQuery({
        queryKey: ['sessions', user?.email], 
        queryFn: async () => {
            const response = await apiFetch("/learningsession");
            const data = await response.json();
            if(!response.ok){
                throw new Error(data.message);
            }
            return data as Array<Session>;
        }
    });

    const startSessionMutation = useMutation(
        {
            mutationFn: async (id: string) => {
                const response = await apiFetch(`/learningsession/${id}`);
                

                if(!response.ok){
                    const data = await response.json()
                    throw new Error(data.message);
                }
                const data = await response.json() as { session: Session, sessionWords: Array<SessionWord> };
                return data;
            },
            onError: data => alert(data.message)
        }
    );

    useEffect(() => {
        if (!startedSessionId) return;

        startSessionMutation.mutate(startedSessionId);
    }, [startedSessionId]);

    useEffect(() => {
        if(!startSessionMutation.data) return;

        sessionDrawerRef.current?.showModal();
    }, [startSessionMutation.data]);

    useEffect(() =>{
        if(getSessionsQuery.data){
            setSessionList(getSessionsQuery.data);
        }
    }, [getSessionsQuery.data]);

    
    const addSessionMutation = useMutation({
        mutationFn: addSession,
        onSuccess: (data) => setSessionList(prev => [...prev, data as Session]),
        onError: (error) => alert(error.message)
    });


    async function addSession(newSession: Omit<Session, "id" | "createdAt">){
        const response = await apiFetch("/learningsession", {
            method: "POST",
            body: JSON.stringify(newSession)
        });

        const data = await response.json();

        if(!response.ok){
            throw new Error(data.message);
        }

        return data;
    }

    function handleAdd(newSession: Omit<Session, "id" | "createdAt">){
        startTransition(() => {
            setOptimisticSessionList(prev => [...prev, {...newSession, createdAt: new Date().toISOString(), id: crypto.randomUUID()}]);
            addSessionMutation.mutate(newSession);
        });
    }


    if(getSessionsQuery.isPending){
        return (
            <section className="sessions area">
                <h2>Learning sessions:</h2>
                <button onClick={() => setIsAdding(true)}>Add session</button>
                <hr />
                <p>Loading...</p>
            </section>
        )
    }

    return (
        <section className="sessions area">
            <h2>Learning sessions:</h2>
            <button onClick={() => setIsAdding(true)}>Add session</button>
            <hr />
            {startedSessionId && startSessionMutation.data && 
            <dialog ref={sessionDrawerRef} onCancel={e => e.preventDefault()}>
                <Session session={startSessionMutation.data.session} sessionWords={startSessionMutation.data.sessionWords} startedSessionId={startedSessionId} setStartedSessionId={setStartedSessionId}/>
            </dialog>}
            { isAdding 
            ? <form onSubmit={(e) => {
                e.preventDefault();
                handleAdd(newSession);
                setIsAdding(false);
            }}>

                <label>Category</label>
                <input type="text" value={newSession.category} onChange={(e) => setNewSession(s => ({...s, category: e.target.value}))} />

                <label>Language</label>
                <input type="text" value={newSession.language} onChange={(e) => setNewSession(s => ({...s, language: e.target.value}))} />
                
                <button type="submit" disabled={addSessionMutation.isPending}>Confirm</button>
                <button type="button" onClick={() => {
                    setIsAdding(false);
                    setNewSession({language: "", category: ""})
                    }}>Close</button>
                
            </form> 
            : <>
            <ul>{[...optimisticSessionList]
                .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
                .map(s => 
                    <SessionCard session={s} setSessionList={setSessionList} setOptimisticSessionList={setOptimisticSessionList} setStartedSessionId={setStartedSessionId} startedSessionId={startedSessionId}/>
            )}</ul></>}
        </section>
    );
}

function SessionCard({ session, setSessionList, setOptimisticSessionList, setStartedSessionId, startedSessionId } : SessionCardProps){
    const sessionCreatedAt = new Date(session.createdAt);
    const [isPending, startTransition] = useTransition();

    const deleteSessionMutation = useMutation({
        mutationFn: deleteSession,
        onSuccess: (sessionId) => { setSessionList(prev => prev.filter(s => s.id !== sessionId)) },
        onError: error => alert(error.message)
    });

    async function deleteSession(sessionId: string){
            const response = await apiFetch(`/learningsession/${sessionId}`, {
                method: "DELETE"
            });
            

            if(!response.ok){
                const data = await response.json();
                throw new Error(data.message);
            }

            return sessionId;
    }
    function handleDelete(sessionId: string){
        startTransition(() => {
            setOptimisticSessionList(prev => prev.filter(s => s.id !== sessionId));
            deleteSessionMutation.mutate(sessionId);
        });

    }

    return(
        <li key={session.id}>
            <div className="session card">
                <span className="id">{session.id}</span>
                <span className="date">{sessionCreatedAt.getDate()}.{sessionCreatedAt.getMonth() + 1}.{sessionCreatedAt.getFullYear()}</span>
                {session.language && <span className="language">{session.language}</span>}
                {session.category && <span className="category">{session.category}</span>}
                <div className="card button">
                    <button onClick={() => {
                        setStartedSessionId(session.id)
                    }} disabled={deleteSessionMutation.isPending || (startedSessionId ? true : false)}>Start</button>
                    <button onClick={() => handleDelete(session.id)} 
                    disabled={deleteSessionMutation.isPending || (startedSessionId ? true : false)}>Delete</button>
                </div>
            </div>
        </li>
    );
}

function Session({ session, sessionWords, startedSessionId, setStartedSessionId } : StartedSessionProps){

    const [words, setWords] = useState([...sessionWords].sort((a, b) => b.order - a.order));
    const [isLastAnswerCorrect, setIsLastAnswerCorrect] = useState<boolean | null>(null);
    const [currentWord, setCurrentWord] = useState<SessionWord | null | undefined>(sessionWords.find(w => w.isCorrect === null));

    const saveAnswerMutation = useMutation({
        mutationFn: async (props: {sessionId: string, wordRequest: {id: string, sessionId: string, userWordId: string, isCorrect: boolean}}) => {
            const response = await apiFetch(`/learningsession/${props.sessionId}`, {
                method: "PUT",
                body: JSON.stringify(props.wordRequest)
            });

            if(!response.ok){
                const data = await response.json();
                throw new Error(data.message);
            }

            return props.wordRequest;
        },
        onSuccess: (wordRequest) => {
            setWords(prev => {
                const updatedWords = prev.map(word =>
                    word.id === wordRequest.id
                        ? { ...word, isCorrect: wordRequest.isCorrect }
                        : word
                );

                if (wordRequest.isCorrect) {
                    const nextWord = updatedWords.find(
                        word => word.isCorrect === null
                    );

                    setCurrentWord(nextWord);
                    setIsLastAnswerCorrect(null);
                }

                return updatedWords;
            });
        },
        onError: (error) => alert(error.message)
    });
    

    function handleAnswer(isCorrect: boolean) {
        if (!currentWord) return;

        const wordRequest = {
            id: currentWord.id,
            userWordId: currentWord.userWordId,
            sessionId: currentWord.sessionId,
            isCorrect: isCorrect
        };

        saveAnswerMutation.mutate({
            sessionId: session.id,
            wordRequest
        });

        setIsLastAnswerCorrect(isCorrect);

    }

    function handleNext(){
        const nextWord = words.find(word => word.isCorrect === null);

        setCurrentWord(nextWord);
        setIsLastAnswerCorrect(null);
    }
    function handleClose(){
        setIsLastAnswerCorrect(null);
        setStartedSessionId(null);
        setCurrentWord(null);
    }



    return( currentWord
    ? (<div>
        <div className="session progress">
            <div className="text"> {words.filter(w => w.isCorrect !== null).length} / {words.length}</div>
            <div className="progress bar">
                <div className="fill" style={{width: `${(words.filter(w => w.isCorrect !== null).length / words.length) * 100}%`}}/>
            </div>
        </div>
        <div className="session">
            <span className="word">{currentWord.word}</span>
            {isLastAnswerCorrect === false && <>
                <span className="translation">{currentWord.translation}</span>
                {currentWord.usageExample && <span className="usageExample">{currentWord.usageExample}</span>}
            </>}
        </div>
        {isLastAnswerCorrect || isLastAnswerCorrect === null
        ? <><button onClick={() => handleAnswer(true)}>Remember</button>
        <button onClick={() => handleAnswer(false)}>Don't remember</button></>
        : <button onClick={() => handleNext()}>Next</button>}
        <button className="closeButton" onClick={() => handleClose()}>Close</button>
    </div>) 
    : (<div>
        <span>You know {words.filter(w => w.isCorrect === true).length} words of {words.length}</span>
        <button className="closeButton" onClick={() => handleClose()}>Close</button>
    </div> ));
}