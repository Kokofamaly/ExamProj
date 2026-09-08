import { useContext, useDeferredValue, useEffect, useOptimistic, useState, useTransition } from "react";
import "./Words.css";
import { useMutation, useQuery, type UseMutationResult } from "@tanstack/react-query";
import { apiFetch } from "./api/apiFetch";
import { jsx } from "react/jsx-runtime";
import { UserContext } from "./UserContext";

interface Word{
    id: string,
    word: string,
    translation: string,
    language: string,
    category?: string,
    usageExample?: string
}


export function Words(){
    const user = useContext(UserContext);
    const [wordList, setWordList] = useState<Array<Word>>([]);
    const [optimisticWordList, setOptimisticWordList] = useOptimistic(wordList);
    const [searchWord, setSearchWord] = useState<string>("");
    const [selectedWordId, setSelectedWordId] = useState<string | null>(null);
    const [mode, setMode] = useState<"editing" | "adding" | null>(null);
    const [editedWord, setEditedWord] = useState<Word | null>(null);

    const [isPending, startTransition] = useTransition();

    const getWordsQuery = useQuery({
        queryKey: ["words", user?.email],
        queryFn: async () => {
            const response = await apiFetch("/userword");
            const data = await response.json();

            if(!response.ok){
                throw new Error(data.message);
            }

            return data;
        }
    });

    useEffect(() =>{
    if(getWordsQuery.data){
        setWordList(getWordsQuery.data);
    }
    }, [getWordsQuery.data]);

    const addWordMutation = useMutation({
        mutationFn: async (newWord: Omit<Word, "id">) => {
            const response = await apiFetch("/userword", {
                method: "POST",
                body: JSON.stringify(newWord)
            });

            const data = await response.json();

            if(!response.ok){
                throw new Error(data.message);
            }

            return data;
        },
        onSuccess: (data) => setWordList(prev => [...prev, data]),
        onError: (error) => alert(error.message),
    });

    const editWordMutation = useMutation({
        mutationFn: async ({wordId, updatedWord}:{wordId: string, updatedWord: Omit<Word, "id">}) => {
            const response = await apiFetch(`/userword/${wordId}`, {
                method: "PUT",
                body: JSON.stringify(updatedWord)
            });
            
            if(!response.ok){
                const data = await response.json();
                throw new Error(data.message);
            }
            return updatedWord;
        },
        onSuccess: (updatedWord: Omit<Word, "id">, variables) => setWordList(prev => prev.map(w => w.id === variables.wordId ? {id: variables.wordId, ...updatedWord} : w)),
        onError: error => alert(error.message)
    });

    const deleteWordMutation = useMutation({
        mutationFn: async (wordId: string) => {
            const response = await apiFetch(`/userword/${wordId}`, { method: "DELETE" });

            if(!response.ok){
                const data = await response.json();
                throw new Error(data.message);
            }
            return wordId;
        },
        onSuccess: wordId => setWordList(prev => prev.filter(w => w.id !== wordId)),
        onError: error => alert(error.message)
    });

    const selectedWord = optimisticWordList.find(w => w.id === selectedWordId);
    const filteredWordList: Array<Word> = wordList.filter(w => w.word.toLowerCase().startsWith(searchWord.toLowerCase()));
    
    function handleEdit(wordId: string, updatedWord: Omit<Word, "id">){
        setMode(null);
        setEditedWord(null);
        startTransition(() =>{
            setOptimisticWordList(prev => prev.map(w => w.id === wordId ? {id: wordId, ...updatedWord} : w));
            editWordMutation.mutate({updatedWord, wordId});
        })
    }

    function handleDelete(wordId: string){
        startTransition(() =>{
            setOptimisticWordList(prev => prev.filter(w => w.id !== wordId));
            deleteWordMutation.mutate(wordId);
        });
    }
    function handleSelect(wordId: string){
        if(mode === "editing") return;
        if(selectedWordId === wordId) {
            setSelectedWordId(null);
            return;
        }
        setSelectedWordId(wordId);
    }
    if(getWordsQuery.isPending){
        return (
            <aside className="words area">
                <h2>Words:</h2>
                <button onClick={() => setMode("adding")} disabled={mode === "adding"}>Add word</button>
                <hr />
                <p>Loading...</p>
            </aside>
        )
    }

    return(
        <aside className="words area">

            <h2>Words:</h2>
            <button onClick={() => setMode("adding")} disabled={mode === "adding"}>Add word</button>
            <hr />
            { mode === "adding" 
            ? <AddForm 
                setMode={setMode} 
                addWordMutation={addWordMutation} 
                setOptimisticWordList={setOptimisticWordList}/> 
            :
            <>
                
                <input 
                    type="text" 
                    placeholder="search..." 
                    value={searchWord} 
                    onChange={e => setSearchWord(e.target.value)}/>
                <ul>
                    {searchWord 
                    ? filteredWordList.map(w => selectedWord === w 
                        ? mode === "editing" 

                            ? <EditWord 
                                word={w} 
                                editedWord={editedWord} 
                                setEditedWord={setEditedWord} 
                                handleEdit={handleEdit} 
                                setMode={setMode} 
                                selectedWordId={selectedWordId}/>

                            : <SelectWord 
                                word={w} 
                                selectedWord={selectedWord} 
                                handleSelect={handleSelect} 
                                handleDelete={handleDelete} 
                                setMode={setMode} 
                                setEditedWord={setEditedWord} /> 

                        : <li key={w.id} className="wordcard" onClick={() => handleSelect(w.id)}>{w.word}</li>) 

                    : optimisticWordList.map(w => selectedWord === w 
                        ? mode === "editing" 

                            ? <EditWord 
                                word={w} 
                                editedWord={editedWord} 
                                setEditedWord={setEditedWord} 
                                handleEdit={handleEdit} 
                                setMode={setMode} 
                                selectedWordId={selectedWordId}/>

                            : <SelectWord 
                                word={w} 
                                selectedWord={selectedWord} 
                                handleSelect={handleSelect} 
                                handleDelete={handleDelete} 
                                setMode={setMode} 
                                setEditedWord={setEditedWord} />

                        : <li key={w.id} className="wordcard" onClick={() => handleSelect(w.id)}>{w.word}</li>)}
                </ul>
            </>}

        </aside>);
}

function AddForm({ setMode, addWordMutation, setOptimisticWordList } : {setMode: (mode: "editing" | "adding" | null) => void, addWordMutation: UseMutationResult<unknown, Error, Omit<Word, "id">, unknown>, setOptimisticWordList: (action: Word[] | ((pendingState: Word[]) => Word[])) => void}){
    const [addedWord, setAddedWord] = useState<Omit<Word, "id">>({word:"", translation:"", language:"", category:"", usageExample:""});
    const [isPending, startTransition] = useTransition();
    
    function handleAdd(newWord: Omit<Word, "id">){
        setMode(null);
        startTransition(() =>{
            setOptimisticWordList(prev => [...prev, {...newWord, id: crypto.randomUUID()}]);
            addWordMutation.mutate(newWord);
        });
    }

    return (<form onSubmit={() => handleAdd(addedWord)}>

                <label>Word</label>
                <input type="text" value={addedWord.word} onChange={e => setAddedWord({...addedWord, word: e.target.value})} required/>

                <label>Translation</label>
                <input type="text" value={addedWord.translation} onChange={e => setAddedWord({...addedWord, translation: e.target.value})} required/>

                <label>Language</label>
                <input type="text" value={addedWord.language} onChange={e => setAddedWord({...addedWord, language: e.target.value})} required/>

                <label>Category</label>
                <input type="text" value={addedWord.category} onChange={e => setAddedWord({...addedWord, category: e.target.value})} />

                <label>Usage example</label>
                <input type="text" value={addedWord.usageExample} onChange={e => setAddedWord({...addedWord, usageExample: e.target.value})} />

                <button type="submit" disabled={addWordMutation.isPending}>Confirm</button>
                <button type="button" onClick={() => setMode(null)}>Close</button>
            </form> );
}

function EditWord({ word, editedWord, setEditedWord, handleEdit, setMode, selectedWordId } : {selectedWordId: string | null, word: Word, editedWord: Word | null, setEditedWord: (value: React.SetStateAction<Word | null>) => void, handleEdit: (wordId: string, updatedWord: Omit<Word, "id">) => void, setMode: (value: React.SetStateAction<"adding" | "editing" | null>) => void}){
    return(
        <li key={word.id}>
            <input type="text" placeholder="word" value={editedWord!.word} onChange={(e) => setEditedWord({...editedWord!, word: e.target.value})}/>
            <input type="text" placeholder="translation" value={editedWord!.translation} onChange={(e) => setEditedWord({...editedWord!, translation: e.target.value})}/>
            <input type="text" placeholder="language" value={editedWord!.language} onChange={(e) => setEditedWord({...editedWord!, language: e.target.value})}/>
            <input type="text" placeholder="category" value={editedWord!.category} onChange={(e) => setEditedWord({...editedWord!, category: e.target.value})}/>
            <input type="text" placeholder="usage example" value={editedWord!.usageExample} onChange={(e) => setEditedWord({...editedWord!, usageExample: e.target.value})}/>
            <button onClick={() => handleEdit(selectedWordId!, editedWord!)}>Confirm</button>
            <button onClick={() => setMode(null)}>Close</button>
        </li>
    );
}

function SelectWord({ word, handleSelect, setMode, setEditedWord, handleDelete, selectedWord} : { handleSelect: (wordId: string) => void,handleDelete: (wordId: string) => void , selectedWord: Word, word: Word, setEditedWord: (value: React.SetStateAction<Word | null>) => void, setMode: (value: React.SetStateAction<"adding" | "editing" | null>) => void}){
    return(
        <li key={word.id} className="wordcard selected" onClick={() => handleSelect(word.id)}>
            <span>{word.word}</span>
            <span>{word.translation}</span>
            <span>{word.language}</span>
            {word.category && <span>{word.category}</span>}
            {word.usageExample && <span>{word.usageExample}</span>}
            <button onClick={(e) => {
                e.stopPropagation();
                setMode("editing");
                setEditedWord(selectedWord);
                }}>Edit</button>
            <button onClick={(e) => {
                e.stopPropagation();
                handleDelete(word.id);
                }}>Delete</button>
        </li> 
    );
}