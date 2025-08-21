// SPDX-FileCopyrightText: 2025 Mnemotechnican
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Floof.Examine;


namespace Content.Server._Floof.Examine;


public sealed class CustomExamineSystem : SharedCustomExamineSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<SetCustomExamineMessage>(OnSetCustomExamineMessage);
    }

    private void OnSetCustomExamineMessage(SetCustomExamineMessage msg, EntitySessionEventArgs args)
    {
        var target = GetEntity(msg.Target);
        if (args.SenderSession.AttachedEntity == null ||
            !CanChangeExamine(args.SenderSession.AttachedEntity.Value, target))
            return;

        var comp = EnsureComp<CustomExamineComponent>(target);

        TrimData(ref msg.PublicData, ref msg.SubtleData);
        comp.PublicData = msg.PublicData;
        comp.SubtleData = msg.SubtleData;

        Dirty(target, comp);
    }
}
