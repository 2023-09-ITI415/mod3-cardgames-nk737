﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Prospector : MonoBehaviour
{

    static public Prospector S;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;

    [Header("Set Dynamically")]
    public Deck deck;
    public Layout layout;
    public List<CardProspector> drawPile;
    public Transform layoutAnchor;
    public CardProspector target;
    [SerializeField] private List<CardProspector> tableau;
    public List<CardProspector> discardPile;

    void Awake()
    {
        S = this;
    }

    void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);   //	This	shuffles	the	deck	by	reference	// a

        Card c;
        for (int cNum = 0; cNum < deck.cards.Count; cNum++)
        {   //	b	
            c = deck.cards[cNum];
            c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        }
        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);

        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
    }
    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card>
     lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardProspector;    //	a	
            lCP.Add(tCP);
        }
        return (lCP);
    }

    //	The	Draw	function	will	pull	a	single	card	from	the	drawPile	andreturn	it
    CardProspector Draw()
    {
        CardProspector cd = drawPile[0];    //	Pull	the	0th	CardProspector	
        drawPile.RemoveAt(0);   //	Then	remove	it	from	List<>	drawPile	
        return (cd);    //	And	return	it	
    }
    //	LayoutGame()	positions	the	initial	tableau	of	cards,	a.k.a.	the "mine"	
    void LayoutGame()
    {
        //	Create	an	empty	GameObject	to	serve	as	an	anchor	for	the	tableau	//	a
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            //	^	Create	an	empty	GameObject	named	_LayoutAnchor	in	the	Hierarchy	
            layoutAnchor = tGO.transform;   //	Grab	its	Transform	
            layoutAnchor.transform.position = layoutCenter; //	Position	it	
        }
        CardProspector cp;
        //	Follow	the	layout	
        foreach (SlotDef tSD in layout.slotDefs)
        {
            //	^	Iterate	through	all	the	SlotDefs	in	the	layout.slotDefs	as	tSD	
            cp = Draw();    //	Pull	a	card	from	the	top	(beginning)	of	the	draw	Pile	
            
            if (cp == null) Debug.Log("cp is null");
            if (tSD == null) Debug.Log("tsD is null");

            cp.faceUp = tSD.faceUp; //	Set	its	faceUp	to	the	value	in	SlotDef	
            cp.transform.parent = layoutAnchor; //	Make	its	parent	layoutAnchor	
                                                //	This	replaces	the	previous	parent:	deck.deckAnchor,	which	
                                                //	appears	as	_Deck	in	the	Hierarchy	when	the	scene	is	playing.	
            cp.transform.localPosition = new Vector3(
            layout.multiplier.x * tSD.x,
            layout.multiplier.y * tSD.y, -tSD.layerID);
            //	^	Set	the	localPosition	of	the	card	based	on	slotDef	
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            //	CardProspectors	in	the	tableau	have	the	state	CardState.tableau	
            cp.state = eCardState.tableau;
            cp.SetSortingLayerName(tSD.layerName);
            tableau.Add(cp);    //	Add	this	CardProspector	to	the	List<>	tableau
        }
        //	Set	which	cards	are	hiding	others	
        foreach (CardProspector tCP in tableau)
        {
            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }
        //	Set	up	the	initial	target	card	
        MoveToTarget(Draw());
        //	Set	up	the	Draw	pile	
        UpdateDrawPile();
        //	Convert	from	the	layoutID	int	to	the	CardProspector	with	that	ID	
       
    }

    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (CardProspector tCP in tableau)
        {
            //	Search	through	all	cards	in	the	tableau	List<>	
            if (tCP.layoutID == layoutID)
            {
                //	If	the	card	has	the	same	ID,	return	it	
                return (tCP);
            }
        }
        //	If	it's	not	found,	return	null	
        return (null);
    }









    //	Moves	the	current	target	to	the	discardPile	
    void MoveToDiscard(CardProspector cd)
        {
            //	Set	the	state	of	the	card	to	discard	
            cd.state = eCardState.discard;
            discardPile.Add(cd);    //	Add	it	to	the	discardPile	List<>	
            cd.transform.parent = layoutAnchor; //	Update	its	transform	parent	
                                                //	Position	this	card	on	the	discardPile	
            cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + 0.5f);
            cd.faceUp = true;
            //	Place	it	on	top	of	the	pile	for	depth	sorting	
            cd.SetSortingLayerName(layout.discardPile.layerName);
            cd.SetSortOrder(-100 + discardPile.Count);
        }



        //	Make	cd	the	new	target	card	
        void MoveToTarget(CardProspector cd)
        {
            //	If	there	is	currently	a	target	card,	move	it	to	discardPile	
            if (target != null) MoveToDiscard(target);
            target = cd;    //	cd	is	the	new	target	
            cd.state = eCardState.target;
            cd.transform.parent = layoutAnchor;
            //	Move	to	the	target	position	
            cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID);
            cd.faceUp = true;   //	Make	it	face-up	
                                //	Set	the	depth	sorting	
            cd.SetSortingLayerName(layout.discardPile.layerName);
            cd.SetSortOrder(0);
        }

        //	Arranges	all	the	cards	of	the	drawPile	to	show	how	many	are	left	
        void UpdateDrawPile()
        {
            CardProspector cd;
            //	Go	through	all	the	cards	of	the	drawPile	
            for (int i = 0; i < drawPile.Count; i++)
            {
                cd = drawPile[i];
                cd.transform.parent = layoutAnchor;

                //	Position	it	correctly	with	the	layout.drawPile.stagger	
                Vector2 dpStagger = layout.drawPile.stagger;
                cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y), -layout.drawPile.layerID + 0.1f * i);
                cd.faceUp = false;  //	Make	them	all	face-down	
                cd.state = eCardState.drawpile;
                //	Set	depth	sorting	
                cd.SetSortingLayerName(layout.drawPile.layerName);
                cd.SetSortOrder(-10 * i);

            cd.faceUp = false;  //	Make	them	all	face-down	
            cd.state = eCardState.drawpile;
            //	Set	depth	sorting	
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
        }



    public void CardClicked(CardProspector cd)
    {
        //	The	reaction	is	determined	by	the	state	of	the	clicked	card	
        switch (cd.state)
        {
            case eCardState.target:
                //	Clicking	the	target	card	does	nothing	
                break;


            case eCardState.drawpile:
                //	Clicking	any	card	in	the	drawPile	will	draw	the	next	card	
                MoveToDiscard(target);  //	Moves	the	target	to	the	discardPile	
                MoveToTarget(Draw());   //	Moves	the	next	drawn	card	to	the	target	
                UpdateDrawPile();   //	Restacks	the	drawPile	
                break;
            case eCardState.tableau:
                //	Clicking	a	card	in	the	tableau	will	check	if	it's	a	valid	play	
                bool validMatch = true;
                if (!cd.faceUp)
                {
                    //	If	the	card	is	face-down,	it's	not	valid	
                    validMatch = false;
                }
                if (!AdjacentRank(cd, target))
                {
                    //	If	it's	not	an	adjacent	rank,	it's	not	valid	
                    validMatch = false;
                }
                if (!validMatch) return;    //	return	if	not	valid	
                                            //	If	we	got	here,	then:	Yay!	It's	a	valid	card.	
                tableau.Remove(cd); //	Remove	it	from	the	tableau	List	
                MoveToTarget(cd);   //	Make	it	the	target	card	
                                    //	Clicking	a	card	in	the	tableau	will	check	if	it's	a	valid	play	
                SetTableauFaces();  //	Update	tableau	card	face-ups	
                break;
        }
        CheckForGameOver();
        //	Test	whether	the	game	is	over	
        void CheckForGameOver()
        { 
                //	If	the	tableau	is	empty,	the	game	is	over	
                if (tableau.Count == 0)
                {
                    //	Call	GameOver()	with	a	win	
                    GameOver(true);
                    return;
                }
                //	If	there	are	still	cards	in	the	draw	pile,	the	game's	not	over	
                if (drawPile.Count > 0)
                {
                    return;
                }
                //	Check	for	remaining	valid	plays	
                foreach (CardProspector cd in tableau)
                {
                    if (AdjacentRank(cd, target))
                    {
                        //	If	there	is	a	valid	play,	the	game's	not	over	
                        return;
                    }
                }
                //	Since	there	are	no	valid	plays,	the	game	is	over	
                //	Call	GameOver	with	a	loss	
                GameOver(false);
            }
            //	Called	when	the	game	is	over.	Simple	for	now,	but	expandable	
            void GameOver(bool won)
            {
                if (won)
                {
                    print("Game	Over.	You	won!	:)");
                }
                else
                {
                    print("Game	Over.	You	Lost.	:(");
                }
                //	Reload	the	scene,	resetting	the	game	
                SceneManager.LoadScene("__Prospector");
            }
        }

    void SetTableauFaces()
    {
        throw new NotImplementedException();
    }

    //	Return	true	if	the	two	cards	are	adjacent	in	rank	(A	&	K	wrap	around)
    public bool AdjacentRank(CardProspector c0, CardProspector c1)
        {
            //	If	either	card	is	face-down,	it's	not	adjacent.	
            if (!c0.faceUp || !c1.faceUp) return (false);
            //	If	they	are	1	apart,	they	are	adjacent	
            if (Mathf.Abs(c0.rank - c1.rank) == 1)
            {
                return (true);
            }
            //	If	one	is	Ace	and	the	other	King,	they	are	adjacent	
            if (c0.rank == 1 && c1.rank == 13) return (true);
            if (c0.rank == 13 && c1.rank == 1) return (true);
            //	Otherwise,	return	false	
            return (false);
        }
    
        
}

